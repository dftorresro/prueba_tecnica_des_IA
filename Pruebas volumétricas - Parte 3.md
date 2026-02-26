 # Parte 3: Estrategia de pruebas volumétricas para la API de facturas

## 1. Qué es una prueba volumétrica:

Una prueba volumétrica busca responder una pregunta muy concreta:

Qué pasa con la aplicación cuando el volumen de datos crece mucho.

A diferencia de una prueba de carga (que se enfoca en muchas peticiones por segundo), una prueba volumétrica se enfoca en escenarios como estos:

- La base de datos tiene millones de facturas.
- Algunos clientes tienen historiales muy grandes.
- Las búsquedas retornan listas largas.
- Los registros son pesados o hay mucha información por transacción.

En mi experiencia, muchos sistemas se ven bien con pocos datos, pero empiezan a degradarse cuando las tablas crecen. Por eso este tipo de prueba es especialmente útil en APIs que dependen de consultas a base de datos.

### Diferencia con carga y estrés:

- Prueba de carga: simula el tráfico esperado del sistema en un día normal o en un pico típico.
- Prueba de estrés: sube el tráfico por encima de lo esperado hasta encontrar el punto donde falla.
- Prueba volumétrica: mantiene un tráfico razonable, pero con un volumen de datos muy grande, para ver si el sistema sigue respondiendo bien.

---

## 2. Escenario de prueba volumétrica:

### Contexto de la API:

El microservicio expone estos endpoints:

- POST /invoice
  Recibe los datos de una factura y los almacena en SQL Server.

- GET /invoice/{id}
  Retorna la información de una factura por identificador.

- GET /invoice/search?client={clientName}
  Retorna las facturas asociadas a un cliente.

### Caso de uso realista:

Un escenario realista es el de una empresa que genera facturas durante el día y luego consulta:

- Facturas puntuales por id (por soporte o auditoría).
- Listados por cliente (por procesos administrativos o validaciones).

Desde el punto de vista volumétrico, el endpoint más sensible suele ser el de búsqueda por cliente, porque puede traer muchos resultados, y eso puede impactar tanto a la base de datos como a la aplicación (memoria, serialización, tamaño de respuesta).

---

## 3. Volúmenes de datos y transacciones a probar:

Para hacer el plan escalable, lo propongo en tres niveles de volumen. La idea es empezar pequeño para validar el proceso y luego subir.

### Dataset S (base):

- 100 mil facturas
- 20 mil clientes

Se usa para validar que el entorno está bien, que el test corre, y que los endpoints funcionan como esperamos.

### Dataset M (volumen serio):

- 5 millones de facturas
- 500 mil clientes

Este nivel ya es suficiente para descubrir problemas típicos de índices, consultas lentas y degradación por tamaño de tablas.

### Dataset L (volumen extremo):

- 50 millones de facturas
- 5 millones de clientes

Aquí se busca ver cómo se comporta el sistema cuando el historial es muy grande, especialmente en búsquedas.

Además de volumen total, es importante definir una distribución realista:

- la mayoría de clientes tiene pocas facturas
- un porcentaje pequeño tiene muchísimas facturas (clientes pesados)

Esto importa porque muchas veces el promedio se ve bien, pero esos casos extremos son los que rompen la aplicación.

### Mezcla de operaciones durante la prueba:

Una mezcla razonable para simular el uso del sistema:

- 70 por ciento GET /invoice/{id}
- 20 por ciento GET /invoice/search?client=
- 10 por ciento POST /invoice

Dentro del search, conviene separar:

- consultas a clientes normales (pocos resultados)
- consultas a clientes pesados (muchos resultados)

---

## 4. Métricas y KPIs propuestos:

Para evaluar el comportamiento del sistema, mediría principalmente:

### Métricas de tiempo de respuesta:

- p50: tiempo típico (la mitad de las requests responden más rápido que esto)
- p95: refleja la experiencia de los casos más lentos
- p99: refleja la cola extrema, útil para detectar pausas por GC, bloqueos o problemas de I/O

Mediría estas métricas por endpoint, no solo un promedio general.

### Métricas de errores:

- porcentaje de respuestas 5xx
- timeouts
- errores por validación (4xx) si aplican
- cualquier incremento de errores al subir el volumen es una señal clara de degradación

### Throughput:

- requests por segundo sostenibles manteniendo latencias aceptables
- especialmente importante cuando el dataset ya es grande

### Métricas de infraestructura:

- CPU
- memoria RAM
- consumo de disco (I/O) si aplica
- señales de saturación (por ejemplo, muchas requests esperando recursos)

### Métricas específicas de SQL Server:

- tiempo de ejecución de stored procedures
- cantidad de lecturas lógicas y físicas
- bloqueos y deadlocks
- uso de índices vs scans

---

## 5. Herramientas para medición:

No es obligatorio usar todas, pero estas son opciones que considero razonables:

### Para generar tráfico :

- k6, JMeter o Locust (cualquiera sirve para enviar requests y medir latencias)

### Para observar la aplicación:

- métricas y dashboards con Prometheus y Grafana (o una alternativa equivalente)
- trazas con OpenTelemetry y Jaeger si se quiere ver el detalle por componente

### Para SQL Server:

- DMVs para diagnóstico (consultas y estado del motor)
- Extended Events para capturar eventos como deadlocks o queries problemáticas

### Para logs:

- Serilog en .NET para generar logs estructurados
- centralización con Grafana Loki o con ELK ( Kibana) según el entorno

---

## 6. Estrategia de ejecución:

Yo lo ejecutaría por fases:

1) Preparar datos
- cargar el dataset S, luego M y luego L
- idealmente sembrar datos directo en SQL Server para hacerlo rápido

2) Medir un baseline con dataset S
- confirmar que todo está estable y que los resultados son coherentes

3) Ejecutar con dataset M
- observar degradación, revisar latencias p95/p99, y revisar comportamiento de SQL

4) Ejecutar con dataset L
- buscar cuellos de botella reales y documentar qué rompe primero

5) Ajustes y repetición
- si se identifica un cuello (por ejemplo, falta de índice o búsqueda sin paginación), proponer solución
- repetir la prueba para confirmar mejora

### Criterios de éxito o fallo (ejemplo):

En un escenario relativaente razonable:

- GET /invoice/{id}
  p95 menor a 200 a 300 ms, error rate menor a 0.1 por ciento

- POST /invoice
  p95 menor a 400 a 600 ms, error rate menor a 0.1 por ciento

- GET /invoice/search
  aquí depende de si hay paginación. Si el endpoint devuelve todo, el criterio debe limitar resultados o proponer paginación.
  con paginación: p95 menor a 500 a 800 ms

Además, el sistema no debería mostrar timeouts, ni errores 5xx al aumentar el volumen.

---

## 7. Posibles cuellos de botella y soluciones:

### Cuellos de botella esperados:

- Búsqueda por clientName lenta si no hay índice o si se usa un patrón de búsqueda no eficiente
- Respuestas enormes en search si no existe paginación o límites
- Stored procedures con joins o filtros que terminan haciendo scans en tablas grandes
- Bloqueos en SQL Server si hay muchas escrituras concurrentes
- Problemas de memoria y tiempos altos al serializar listas grandes de facturas
- saturación del pool de conexiones si la API abre demasiadas conexiones o no gestiona bien los recursos

### Soluciones típicas:

- crear índices adecuados (por ejemplo por invoiceId, clientId, createdAt)
- preferir buscar por clientId en lugar de clientName si es posible
- introducir paginación y límites máximos en el endpoint de búsqueda
- optimizar stored procedures (filtros sargables, evitar scans)
- mejorar manejo de conexiones y timeouts
- reducir el tamaño de respuesta (solo campos necesarios) si aplica

---

## Comentario :

Con este plan, el objetivo es asegurar que la API no solo funciona, sino que sigue funcionando bien cuando el sistema crece, que es donde normalmente aparecen los problemas reales.

Si yo tuviera que priorizar, empezaría por el endpoint de búsqueda por cliente y por validar que la base de datos tiene índices adecuados, porque ahí suele aparecer la degradación más rápido.