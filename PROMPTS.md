# PROMPTS.md

## Objetivo:
Optimizar el prompt de resumen para que la salida sea breve, precisa y consistente, enfocada en los puntos contractuales clave del texto (determinados posteriormente), minimizando divagaciones, omisiones y alucinaciones.

---

## 1) Mejoras realizadas y por qué:

### 1. Control explícito de longitud y densidad informativa
**Cambio:** Establecí un máximo estricto (ej. **6 viñetas**, **1 línea por viñeta**, sin subtítulos adicionales).  
**Por qué:** “Corto” es una caracteristica subjetiva. Un límite cuantitativo reduce variabilidad, fuerza priorización y evita explicaciones extensas.

### 2. Estructura obligatoria orientada a requerimientos :
**Cambio:** Definí un formato fijo y predecible por categorías: **plazos/avisos**, **cobertura/ monto**, **vigencia**, **pago/cesión**, **exclusiones**, **nota regulatoria**.  
**Por qué:** El texto legal tiene muchas piezas; sin estructura, el modelo suele:
- Omitir plazos.
- Mezclar conceptos.
- Dedicar espacio a detalles secundarios.

### 3. Criterios de inclusión:
**Cambio:** Enumeré los “must  have” del resumen:
- Monto asegurado
- Coberturas principales (muerte e incapacidad total y permanente)
- Plazos relevantes (30 días, 10 días hábiles, etc.)
- Inicio de vigencia.
- Forma de pago.
- Cesión por titularización.
- Dónde están las exclusiones.
- Circular 028/2019.
**Por qué:** Un resumen correcto para un evaluador no es solo corto, debe preservar información crítica.

### 4. Reglas explícitas de exclusión o no relevancia:
**Cambio:** Prohibí:
- Definiciones (“qué significa beneficiario oneroso”).
- Recomendaciones (“deberías…”).
- Interpretación legal.
- Reescritura extensa.
- Contenido fuera del texto.  
**Por qué:** En textos legales, los modelos tienden a explicar o aconsejar, eso agrega ruido y reduce precisión.

### 5. Fidelidad al texto y manejo de incertidumbre:
**Cambio:** Instrucción de seguridad:
- “No inventes datos” (ej. edad máxima exacta si no está en el texto).  
- Si no está explícito, se debe omitit (o marcar como “no especificado”).  
**Por qué:** Reduce alucinaciones y evita errores factuales.

### 6. Reglas de estilo para evitar relleno
**Cambio:** Añadí restricciones de estilo:
- Lenguaje neutral (sin introducciones tipo “Este texto trata de…”).
- Sin redundancias.
- Sin ejemplos extensos.  
**Por qué:** Mantiene alta densidad informativa y evita frases genéricas.
---

## 2) Prompt optimizado:

> **Rol:** Eres un asistente de redacción legal.  
> **Tarea:** Resume el texto a continuación en **español**, con tono **neutral** y **factual**.  
> 
> **Formato obligatorio (exactamente 6 viñetas, 1 oración por viñeta):**
> 1) **Avisos y plazos:** cambios o revocación y siniestros (incluye 30 días y 10 días hábiles, y a quién se notifica).  
> 2) **Terminación por mora:** aviso y cobertura durante el periodo informado.  
> 3) **Cobertura y monto:** valor asegurado y coberturas principales (muerte e incapacidad total y permanente).  
> 4) **Inicio de vigencia:** desde cuándo aplica la cobertura.  
> 5) **Pago y cesión:** periodicidad, medio de pago y cesión por titularización (si aplica).  
> 6) **Exclusiones y nota regulatoria:** dónde están las exclusiones y mención de Circular Externa.
> 
> **Reglas estrictas:**
> - No expliques términos, no aconsejes, no interpretes.  
> - No copies frases largas del texto; parafrasea.  
> - No incluyas información no presente en el texto.  
> - Si un dato no está explícito, no lo inventes (omítelo).  
> - Mantén cada viñeta en máximo 25 palabras.
> 
> **Texto a resumir:**  
> [PEGA AQUÍ EL TEXTO COMPLETO]

---

## 3) Cómo esta versión evita respuestas irrelevantes

- **Formato fijo + número exacto de viñetas:** elimina introducciones, conclusiones y explicaciones.  
- **Checklist embebido (los 6 temas):** reduce omisiones de puntos contractuales clave.  
- **Reglas de exclusión:** bloquean definiciones, consejos y contexto extra no solicitado.  
- **Restricción por viñeta ( <= 25 palabras):** obliga a comprimir y evita listas largas de ejemplos.  
- **Regla de fidelidad (“no inventar”):** minimiza alucinaciones (ej. edad máxima exacta).

---

## 4) Ejemplo comparativo (original vs mejorado) 

### 4.1 Prompt original
**Prompt:**  
"Resume el siguiente texto: [ ... ]. Devuelve solo un resumen corto y preciso."

**Problemas concretos del original (por qué puede fallar):**
1. **Ambigüedad de longitud:** “corto” puede ser 2 líneas o 2 párrafos.
2. **Sin estructura:** el modelo decide qué es importante - > riesgo alto de omitir plazos o mora.
3. **Sin reglas anti-irrelevancia:** puede incluir explicaciones legales (“esto significa que…”).
4. **Sin control de cobertura mínima:** puede enfocarse en coberturas y olvidar pago/cesión/exclusiones.
5. **Mayor riesgo de alucinación:** puede “completar” datos no dados (edad máxima, condiciones adicionales).
6. **Puede copiar texto:** sin restricción, el modelo puede devolver un “resumen” que es casi extractivo.

**Ejemplo de salida típica problemática (ilustrativo):**
- “Este seguro ofrece protección integral y cubre múltiples eventos…” (frases vagas)
- Explica qué es “beneficiario oneroso”
- Omite: “Sura avisará al banco dentro de 10 días hábiles” o el esquema de mora
- Se va a 200–300 palabras

---

### 4.2 Prompt mejorado
**Prompt:** (el de la sección 2)

**Cómo cambia el comportamiento del modelo (comparación punto por punto):**

1. **Longitud**
   - *Original:* “corto” sin métrica - > longitud variable.
   - *Mejorado:* 6 viñetas, 1 oración, <= 25 palabras - > longitud estable y controlada.

2. **Cobertura mínima garantizada**
   - *Original:* el modelo decide prioridades.
   - *Mejorado:* los 6 ítems funcionan como checklist: plazos, mora, cobertura/monto, vigencia, pago/cesión, exclusiones/circular.

3. **Evita relleno**
   - *Original:* probable introducción (“el objetivo del documento…”).
   - *Mejorado:* formato directo, sin espacio para prefacios ni explicaciones.

4. **Reduce detalles secundarios**
   - *Original:* puede listar muchos eventos (homicidio, terrorismo, etc.) consumiendo espacio.
   - *Mejorado:* obliga a agrupar (“muerte por cualquier causa, incluyendo eventos enumerados”) manteniendo precisión.

5. **Mayor fidelidad**
   - *Original:* puede inferir o añadir interpretaciones.
   - *Mejorado:* reglas “no interpretes / no inventes / omite lo no explícito” reducen errores.

---

### 4.3 Ejemplo de salida esperada con el prompt mejorado 

1) **Avisos y plazos:** revocación/modificación se notifica al BANCO por escrito con >= 30 días; siniestro se reporta a Sura dentro de 30 días; Sura avisa al BANCO en 10 días hábiles.  
2) **Terminación por mora:** si hay terminación automática por no pago, se informa por escrito al beneficiario oneroso con hasta 30 días de antelación y se garantiza cobertura en ese periodo.  
3) **Cobertura y monto:** valor asegurado de $98.500.000 por fallecimiento; cubre muerte por cualquier causa bajo condiciones del seguro.  
4) **Inicio de vigencia:** rige desde las 24:00 del día de expedición indicado en la carátula de la póliza.  
5) **Pago y cesión:** pago anual por cobro bancario; puede cederse por titularización de cartera y la cesión debe notificarse.  
6) **Exclusiones y nota regulatoria:** exclusiones generales en el clausulado y particulares en la carátula; Circular 028 de 2019 permite que la entidad financiera pague la prima para evitar terminación automática.

---

## 5) Criterios de evaluación (cómo medir que el prompt es mejor)

**Precisión y fidelidad**
- No inventa condiciones ni agrega recomendaciones.

**Cobertura mínima**
- Siempre incluye lo necesario: monto, plazos, mora, vigencia, pago, cesión, exclusiones, circular.

**Concisón**
- Mantiene límites de longitud y fromato.

**Relevancia**
- Evita explicaciones y meta texto.

**Consistencia**
- Diferentes ejecuciones del prompt producen salidas con estructura y extensión similares.

## Comentario

En este caso, no intenté crear un prompt universal que funcione perfecto para cualquier tipo de resumen sin cambios. Lo que busqué fue demostrar que sé adaptara un prompt al tipo de texto que se tiene.

Este contenido lo interprete como una póliza condiciones de seguro, y en este tipo de documentos lo más importante suele ser no omitir datos clave y no inventar ni dar opiniones. Por eso mi versión fuerza una estructura y limita la salida para que el resumen sea directo y relevante.

Dicho esto, la forma del prompt sí es reutilizable: siempre uso la misma idea de definir longitud, formato, qué incluir y qué evitar. Lo que realmente cambia según el caso es la lista de puntos obligatorios. Si en otrro caso el texto fuera una noticia o un artículo científico, mantendría la estructura, pero ajustaría los elementos que deben aparecer en el resumen.