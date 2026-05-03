# Estándares Clínicos - Perfil Médico Especializado

La arquitectura y diseño de `MedFarLab` se utiliza en ambientes hospitalarios, laboratorios y farmacias (Clínicos). Las interfaces deben evaluarse bajo una lupa estrictamente **Médica** centrada en la "Seguridad del Paciente" y la usabilidad de alta intensidad.

## 1. Densidad y Jerarquía de la Información Clínica
- **Escaneo Visual Rápido:** Los médicos y técnicos disponen de pocos segundos para revisar historiales clínicos. Los diagnósticos críticos, alergias, o resultados críticos de laboratorio **deben resaltar instantáneamente** (ej: Badges rojos de alta visibilidad siempre fijados en la cabecera de la ficha del paciente).
- **Reducción de Fatiga Visual (Dark Mode Completo):** En áreas de radiología, ultrasonido o laboratorios de análisis, los monitores operan a veces con baja luz ambiental. Todo el sistema debe soportar *Dark Mode* ergonómico y con contraste amigable.
- **Evitar la Ocultación Innecesaria:** Si una información es un "Signo Vital Crítico" (Presión arterial alta en alerta roja), NO debe quedar oculta bajo pestañas expansibles o menús *dropdown*. Las pantallas médicas privilegian la densidad controlada (muchos datos en una vista tabulada clara) por encima del diseño exageradamente minimalista que requeriría 4 clics extra.

## 2. Prevención de Errores (Poka-Yoke Clínico)
- **Bloqueos Duros y Suaves:** 
  - Si un médico intenta recetar Penicilina a un paciente registrado con alergia a la misma, el Frontend debe emitir un modal rojo de advertencia severa que requiera confirmación explícita mediante un código (Soft Block) o simplemente no permitir el guardado de la receta (Hard Block).
- **Manejo de Unidades de Medida Estándar:** Jamás presentar valores crudos en las pantallas de resultados de laboratorio sin especificar la métrica (`mg/dL`, `mmol/L`).
- **Nombres Similares (LASA - Look Alike, Sound Alike):** Tanto para pacientes como medicamentos. Si en el listado del día de admisiones hay pacientes con nombres casi idénticos (Ej: Juan Pérez Morales vs Juan Pérez Rodríguez), el sistema de diseño debe resaltar la fecha de nacimiento u otro ID secundario visualmente para evitar confundir la ficha al hacer clic.

## 3. Revisión Médica Continua (Checklist de Mejora de Vistas)
Cualquier diseñador/desarrollador de MedFarLab debe someter las vistas a esta batería de preguntas (Revisión Clínica):
1. *¿Si el paciente está en urgencias, a cuántos clics estoy de su tipo de sangre y alergias?*
2. *¿El formato de las prescripciones y dosis previene que un "10.0 mg" se lea por error como "100 mg"?* (Uso de ceros precedentes y evasión de ceros finales innecesarios).
3. *¿La firma de los resultados de laboratorio permite el flujo de aprobación dual (Técnico prepara, Patólogo valida)?*
