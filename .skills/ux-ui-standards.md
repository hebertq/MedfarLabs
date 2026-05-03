# Estándares de Diseño - Perfil UX/UI

MedFarLab se orienta a ser una plataforma "Premium" que deslumbre al usuario en todo momento (`Wow factor`). Estos son los estándares visuales a seguir para el Frontend (`PWA` y `WebUI`).

## 1. Diseño Dinámico y Estética Premium
- **Cero Elementos "Planos por Defecto":** Los botones primarios nunca deben tener un color azul genérico estándar. Utilizar gradientes sutiles (ej: LinearGradient con `var(--primary-color)` a `var(--primary-light)`), esquinas ligeramente redondeadas (`border-radius: 8px` a `12px`), y sombras de caja de profundidad múltiple (`box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1)`).
- **Glassmorphism Inteligente:** Para modales, *Sidebars* y *Navbars*, emplear filtros de desenfoque (`backdrop-filter: blur(10px)`) combinados con fondos semi-transparentes (`background: rgba(255, 255, 255, 0.8)`) para un look moderno de "vidrio esmerilado", respetando los contrastes de legibilidad.
- **Tipografía Moderna:** Usar tipografías claras y corporativas como `Inter`, `Roboto` u `Outfit`. Respetar jerarquías (H1 grande, H2 con peso semi-bold, texto de párrafo de 14px a 16px).

## 2. Micro-Interacciones (Comportamiento y "Feels Alive")
- Todo elemento accionable (Botón, Fila de tabla cliqueable, Tarjeta) debe tener un estado `:hover` con transición suave (`transition: all 0.2s ease-in-out;`).
  - Ejemplo: Un botón al pasar el cursor (Hover) puede hacer un ligero `transform: translateY(-2px)` y aumentar ligeramente el *box-shadow*.
  - En estado `:active` (Click) reducir la escala: `transform: scale(0.98)`.

## 3. Esqueletos y Estados de Espera
- **PROHIBIDO mostrar la pantalla en blanco** durante las cargas asíncronas de Blazor.
- Si una tabla o lista de datos está cargando, utilizar **Skeleton Loaders** (estructuras grises animadas que simulan el contenido antes de que llegue) en lugar de *spinners* giratorios gigantes.

## 4. Respuestas Visuales de Éxito / Error
- Usar el componente *Toaster* (Modales no intrusivos que aparecen en la esquina superior) en vez de "Alerts" horribles nativas del navegador.
- Si una tabla de datos está vacía, mostrar un **Empty State Empático**: Un ícono ilustrativo bonito, un título amigable ("No hemos encontrado facturas este mes") y un botón ("Crear nueva factura") para guiar al usuario a la acción.

## 5. Uniformidad en Navegación Interna (Page Headers y Diálogos)
Para evitar experiencias desarticuladas en pantallas de detalle:
- Toda página secundaria o de detalle debe estar coronada por un **Header de Acciones Estándar** (Action Bar) consistente.
- Los botones de control primarios (`Regresar`, `Guardar`, `Actualizar`, `Imprimir`, `Anular`) deben residir obligatoriamente en esta barra superior, siempre con el mismo orden y color semántico.
- **Limpieza visual:** Acciones raras o secundarias nunca deben saturar esta barra; deben condensarse dentro de un "Menú de 3 puntos" (Menú Kebab) ubicado al extremo derecho.
- Las ventanas modales/diálogos también deben aplicar este Header como cabecera del modal para asegurar que un usuario sepa intuitivamente dónde encontrar los botones de guardar y cerrar.
