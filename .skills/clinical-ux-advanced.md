### 📂 Archivo 2: `clinical-ux-advanced.md`

Este documento se centra en la **Seguridad del Paciente** y la eficiencia del médico en entornos de estrés.

```markdown
# Estándares Clínicos Avanzados - Perfil de Seguridad del Paciente

Define las reglas de interfaz para prevenir errores médicos y maximizar el escaneo visual de datos críticos.

## 1. Jerarquía de Alertas Fijas (Sticky Badges)
- **Regla de los 2 Segundos:** Un médico debe identificar Alergias y Diagnósticos Críticos en menos de 2 segundos al abrir una ficha.
- **Cabecera Persistente:** Los Badges de "Alergias" (Rojo) y "Riesgos" (Naranja) deben estar fijos en la parte superior derecha de la ficha del paciente y no desaparecer con el scroll.

## 2. Bloqueos de Seguridad (Poka-Yoke Clínico)
- **Validación Pre-Guardado:** El botón "Guardar" de una receta o cierre de consulta debe disparar un chequeo cruzado:
  - *Medicamento vs Alergias:* Si hay coincidencia, disparar un **Soft Block** (Modal de advertencia con confirmación doble).
  - *Dosis Inusual:* Si la dosis está fuera de los rangos estándar del catálogo, pedir confirmación del médico.
- **Cero Ambigüedad:** En visualización de resultados de laboratorio, los valores fuera de rango deben estar en **Negrita y Rojo**, acompañados de una flecha de tendencia (↑/↓).

## 3. Densidad para Áreas Especializadas
- **Modo Laboratorio/Radiología:** Las pantallas de estos módulos deben permitir una densidad de datos 20% mayor a la estándar, eliminando espacios en blanco innecesarios para evitar que el técnico deba hacer scroll para ver resultados relacionados.
