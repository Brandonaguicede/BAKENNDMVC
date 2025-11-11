// ==========================================================
//  INICIALIZACIÓN PRINCIPAL
// ==========================================================
document.addEventListener('DOMContentLoaded', function () {
    console.log("🚀 Inicializando aplicación de diseño...");

    // Llama a las funciones principales que preparan la interfaz
    cargarCategorias();
    initializeViews();
    initializeSizeSelection();
    initializeTools();
    initializeBottomTools();
    initializeInfoModal();



});

// ==========================================================
//  CARGAR CATEGORÍAS DESDE LA BASE DE DATOS
// ==========================================================
// Variable global para almacenar las categorías obtenidas del servidor
let categoriasGlobal = [];



// =======================================================================
// Función principal que obtiene las categorías y las muestra en pantalla
// =======================================================================
async function cargarCategorias() {

    // Busca el contenedor HTML donde se mostrarán las categorías
    const contenedor = document.getElementById("categoriasContainer");

    // Si no se encuentra el contenedor, se muestra un error en la consola
    if (!contenedor) {
        console.error("❌ No se encontró el contenedor de categorías");
        return;
    }

    console.log("📦 Cargando categorías desde el servidor...");

    try {
        // Realiza una solicitud HTTP GET al backend para obtener las categorías
        const response = await fetch("/Categorias/ObtenerCategorias");

        // Si la respuesta del servidor no es correcta lanza un error
        if (!response.ok) {
            throw new Error(`Error HTTP: ${response.status}`);
        }

        // Convierte la respuesta JSON en un array de objetos js
        const categorias = await response.json();

        // Guarda las categorías globalmente para poder reutilizarlas
        categoriasGlobal = categorias;
        console.log("✅ Categorías obtenidas:", categorias);


        // Si no hay categorías disponibles, muestra un mensaje en el contenedor
        if (!categorias || categorias.length === 0) {
            contenedor.innerHTML = `
                <p style="color: #999; text-align: center; padding: 15px; font-size: 14px;">
                    📁 No hay categorías disponibles
                </p>
            `;
            return;
        }


        // Crea los botones HTML de cada categoría usando .map()
        contenedor.innerHTML = categorias.map(c => `
            <button class="tool-button categoria-btn" data-id="${c.id}">
                📁 ${c.nombre}
            </button>
        `).join("");

        // Agrega un evento de clic a cada botón generado
        document.querySelectorAll(".categoria-btn").forEach(btn => {
            btn.addEventListener("click", function () {

                // Quita la clase "active" de todos los botones
                document.querySelectorAll(".categoria-btn").forEach(b =>
                    b.classList.remove("active")
                );

                // Marca este botón como activo
                this.classList.add("active");

                // Obtiene el ID y nombre de la categoría seleccionada
                const categoriaId = this.dataset.id;
                const categoriaNombre = this.textContent.trim();


                // Muestra qué categoría se seleccionó
                console.log(`✅ Categoría seleccionada: ${categoriaNombre} (ID: ${categoriaId})`);
            });
        });

        // Llama a otra función que cambia el producto o recarga los productos según la categoría
        initializeCambiarProducto();

        console.log(`✅ ${categorias.length} categorías cargadas exitosamente`);

    } catch (error) {

        // se muestra el mensaje de error 
        console.error("❌ Error cargando categorías:", error);
        contenedor.innerHTML = `
            <div style="color: #e74c3c; padding: 15px; text-align: center; font-size: 14px;">
                <p style="margin-bottom: 5px;">⚠️ Error al cargar categorías</p>
                <small style="color: #999;">${error.message}</small>
            </div>
        `;
    }
}


// ==========================================================
//  Funsión para cambiar de categoría
// ==========================================================
function initializeCambiarProducto() {

    // Busca el botón que permite cambiar de producto (por su ID)
    const changeProductBtn = document.getElementById('change-product-btn');
    if (!changeProductBtn) return;


    // Si el botón sí existe, le asigna un evento al hacer clic
    // Al presionar, se mostrará el modal de categorías
    changeProductBtn.addEventListener('click', mostrarModalCategorias);
}



// ==========================================================
//  Funsion de mostrar categorias 
// ==========================================================
function mostrarModalCategorias() {

    // Busca el modal de categorías en el documento
    const modalElement = document.getElementById('modalCategorias');
    if (!modalElement) return;

    // Crea una instancia de Bootstrap Modal con ciertas configuraciones:
    //  backdrop: 'static' que evita que el usuario cierre el modal haciendo clic fuera
    //  keyboard: false que evita que se cierre presionando la tecla ESC
    const modalBootstrap = new bootstrap.Modal(modalElement, {
        backdrop: 'static',
        keyboard: false
    });

    //muestra el mpodal
    modalBootstrap.show();
}

function showNotification(message) {

    // Crea dinámicamente un nuevo elemento <div> que servirá como notificación
    const notification = document.createElement('div');
    notification.className = 'notification';
    notification.textContent = message;
    document.body.appendChild(notification);

    // Después de 3 segundos (3000 milisegundos), elimina la notificación del DOM
    setTimeout(() => notification.remove(), 3000);
}



// ==========================================================
//  CAMBIO DE VISTAS (FRENTE/ESPALDA)
// ==========================================================
function initializeViews() {
    document.querySelectorAll('.view-button').forEach(button => {
        button.addEventListener('click', function () {
            document.querySelectorAll('.view-button').forEach(btn => btn.classList.remove('active'));
            document.querySelectorAll('.lawView').forEach(view => view.classList.remove('active'));

            this.classList.add('active');

            // Obtiene el valor de data-view (por ejemplo 'front' o 'back')
            const view = this.getAttribute('data-view');

            // Busca el elemento de la vista correspondiente usando el ID
            const viewElement = document.getElementById(view + '-view');
            if (viewElement) {
                viewElement.classList.add('active');
            }
        });
    });
}



// ==========================================================
//  SELECCIÓN DE TALLAS
// ==========================================================================================

function initializeSizeSelection() {
    console.log('🎯 Inicializando selección de tallas...');

    // Obtener la talla activa por defecto
    const tallaActiva = document.querySelector('.size-option.active');
    if (tallaActiva) {
        tallaSeleccionada = tallaActiva.textContent.trim();
        console.log('✅ Talla inicial detectada:', tallaSeleccionada);
    }

    // Agregar evento a cada opción de talla
    document.querySelectorAll('.size-option').forEach(option => {
        option.addEventListener('click', function () {
            // Remover active de todas
            document.querySelectorAll('.size-option').forEach(opt => opt.classList.remove('active'));

            // Activar la seleccionada
            this.classList.add('active');

            // Guardar la talla
            tallaSeleccionada = this.textContent.trim();
            console.log('✅ Nueva talla seleccionada:', tallaSeleccionada);
        });
    });
}
// Exportar función
window.getTallaSeleccionada = function () {
    console.log('📏 getTallaSeleccionada llamada, talla actual:', tallaSeleccionada);
    return tallaSeleccionada;
};




// ==========================================================
//  AÑADIR IMAGEN - CORREGIDA
// ==========================================================
// Actualiza esta sección en tu función initializeTools()
function initializeTools() {
    document.querySelectorAll('.tool-button').forEach(button => {
        button.addEventListener('click', function () {
            if (this.getAttribute('data-tool')) {
                document.querySelectorAll('.tool-button').forEach(btn => btn.classList.remove('active'));
                this.classList.add('active');

                document.querySelectorAll('.text-editor').forEach(editor => editor.classList.remove('active'));

                const tool = this.getAttribute('data-tool');
                if (tool === 'text') {
                    const textEditor = document.getElementById('text-editor');
                    if (textEditor) textEditor.classList.add('active');
                }
            }
        });
    });

    // BOTÓN PARA AÑADIR IMAGEN
    const addImageBtn = document.getElementById('add-image-btn');
    if (addImageBtn) {
        addImageBtn.addEventListener('click', function () {
            const imageUpload = document.getElementById('image-upload');
            if (imageUpload) imageUpload.click();
        });
    }

    // INPUT DE SUBIDA DE IMAGEN - CORREGIDO
    const imageUpload = document.getElementById('image-upload');
    if (imageUpload) {
        imageUpload.addEventListener('change', function (e) {
            const file = e.target.files[0];
            if (!file) return;

            if (!file.type.startsWith('image/')) {
                alert('Por favor, selecciona un archivo de imagen válido.');
                return;
            }

            const reader = new FileReader();
            reader.onload = function (event) {
                // 🎯 CAMBIO: Usar design-elements-layer en lugar de content-inner
                const activeView = document.querySelector('.lawView.active .design-elements-layer');
                if (!activeView) {
                    console.error('No se encontró el área de diseño activa');
                    return;
                }

                // 🎯 Obtener el overlay para centrar
                const overlay = document.querySelector('.lawView.active .design-area-overlay');
                if (!overlay) {
                    console.error('No se encontró el overlay de diseño');
                    return;
                }

                // Calcular centro del overlay
                const overlayRect = overlay.getBoundingClientRect();
                const parentRect = activeView.getBoundingClientRect();

                const centroX = overlayRect.left - parentRect.left + (overlayRect.width / 2);
                const centroY = overlayRect.top - parentRect.top + (overlayRect.height / 2);

                const wrapper = document.createElement('div');
                wrapper.className = 'arrastrable-escalable imagen-arrastrable';

                // 🎯 POSICIÓN CENTRADA
                wrapper.style.cssText = `
                    position: absolute;
                    left: ${centroX - 50}px;
                    top: ${centroY - 50}px;
                    width: 100px;
                    height: 100px;
                    z-index: 200;
                    cursor: move;
                `;

                const img = document.createElement('img');
                img.src = event.target.result;
                img.style.cssText = `
                    width: 100%;
                    height: 100%;
                    object-fit: contain;
                    pointer-events: none;
                `;

                wrapper.appendChild(img);
                activeView.appendChild(wrapper);
                hacerArrastrableYEscalable(wrapper);

                console.log('✅ Imagen cargada en el centro del área de diseño');
            };

            reader.onerror = function () {
                console.error('Error al leer el archivo');
                alert('Error al cargar la imagen. Inténtalo de nuevo.');
            };

            reader.readAsDataURL(file);
            e.target.value = '';
        });
    }

    // BOTÓN PARA AÑADIR TEXTO
    const addTextBtn = document.getElementById('add-text-btn');
    if (addTextBtn) {
        addTextBtn.addEventListener('click', addTextToDesign);
    }
}



// ==========================================================
//  FUNCIÓN PARA AÑADIR TEXTO AL DISEÑO - CORREGIDA
// ==========================================================
function addTextToDesign() {
    const textInput = document.getElementById('design-text');
    const colorInput = document.getElementById('text-color');
    const fontInput = document.getElementById('font-family');

    if (!textInput || !colorInput || !fontInput) {
        console.error('Elementos de texto no encontrados');
        return;
    }

    const text = textInput.value.trim();
    const color = colorInput.value;
    const font = fontInput.value;

    if (text === '') {
        alert('Por favor, escribe algún texto primero.');
        return;
    }

    // 🎯 CAMBIO IMPORTANTE: Seleccionar la capa de elementos editables
    const activeView = document.querySelector('.lawView.active .design-elements-layer');
    if (!activeView) {
        console.error('No se encontró el área de diseño activa');
        return;
    }

    // 🎯 Obtener el área de diseño overlay para centrar el elemento
    const overlay = document.querySelector('.lawView.active .design-area-overlay');
    if (!overlay) {
        console.error('No se encontró el overlay de diseño');
        return;
    }

    // Calcular el centro del overlay
    const overlayRect = overlay.getBoundingClientRect();
    const parentRect = activeView.getBoundingClientRect();

    // Posición centrada relativa al parent
    const centroX = overlayRect.left - parentRect.left + (overlayRect.width / 2);
    const centroY = overlayRect.top - parentRect.top + (overlayRect.height / 2);

    const textElement = document.createElement('div');
    textElement.className = 'arrastrable-escalable texto-arrastrable';

    // 🎯 POSICIÓN CENTRADA EN EL OVERLAY
    textElement.style.cssText = `
        position: absolute;
        left: ${centroX - 50}px;
        top: ${centroY - 20}px;
        color: ${color};
        font-family: ${font};
        font-size: 18px;
        font-weight: bold;
        cursor: move;
        z-index: 200;
        padding: 10px;
        background: transparent;
        border-radius: 5px;
        user-select: none;
    `;

    textElement.textContent = text;
    activeView.appendChild(textElement);
    hacerArrastrableYEscalable(textElement);

    textInput.value = '';
    console.log('✅ Texto añadido en el centro del área de diseño');
}

function hacerArrastrableYEscalable(elemento) {
    let isDragging = false;
    let isResizing = false;
    let isRotating = false;
    let offsetX, offsetY, startX, startY, startWidth, startHeight;

    elemento.style.position = "absolute";
    elemento.style.cursor = "move";
    elemento.style.userSelect = "none";
    elemento.style.transition = "transform 0.1s ease";

    // Contenedor de controles
    const controlsContainer = document.createElement("div");
    controlsContainer.style.cssText = `
        position: absolute;
        inset: 0;
        pointer-events: none;
    `;

    // Botón de rotación
    const rotateHandle = document.createElement("div");
    rotateHandle.innerHTML = "↻";
    rotateHandle.style.cssText = `
        position: absolute;
        top: -25px;
        left: 50%;
        transform: translateX(-50%);
        font-size: 18px;
        background: white;
        border: 1px solid #ccc;
        border-radius: 50%;
        width: 25px;
        height: 25px;
        display: flex;
        align-items: center;
        justify-content: center;
        pointer-events: auto;
        cursor: grab;
        box-shadow: 0 2px 4px rgba(0,0,0,0.15);
    `;

    // Botón de redimensionar
    const resizeHandle = document.createElement("div");
    resizeHandle.innerHTML = "↔";
    resizeHandle.style.cssText = `
        position: absolute;
        right: -12px;
        bottom: -12px;
        font-size: 14px;
        background: white;
        border: 1px solid #ccc;
        border-radius: 50%;
        width: 22px;
        height: 22px;
        display: flex;
        align-items: center;
        justify-content: center;
        pointer-events: auto;
        cursor: se-resize;
        box-shadow: 0 2px 4px rgba(0,0,0,0.15);
    `;

    // Botón de eliminar
    const deleteHandle = document.createElement("div");
    deleteHandle.innerHTML = "🗑️";
    deleteHandle.style.cssText = `
        position: absolute;
        top: -25px;
        right: -25px;
        font-size: 18px;
        background: white;
        border: 1px solid #ccc;
        border-radius: 50%;
        width: 28px;
        height: 28px;
        display: flex;
        align-items: center;
        justify-content: center;
        pointer-events: auto;
        cursor: pointer;
        box-shadow: 0 2px 4px rgba(0,0,0,0.15);
    `;

    controlsContainer.appendChild(rotateHandle);
    controlsContainer.appendChild(resizeHandle);
    controlsContainer.appendChild(deleteHandle);
    elemento.appendChild(controlsContainer);

    // 🎯 Obtener límites del área de diseño
    function getLimites() {
        const overlay = document.querySelector('.lawView.active .design-area-overlay');
        const parent = elemento.parentElement;

        if (overlay && parent) {
            const overlayRect = overlay.getBoundingClientRect();
            const parentRect = parent.getBoundingClientRect();

            return {
                minX: overlayRect.left - parentRect.left,
                maxX: overlayRect.right - parentRect.left,
                minY: overlayRect.top - parentRect.top,
                maxY: overlayRect.bottom - parentRect.top
            };
        }
        return null;
    }

    // MOVIMIENTO CON LÍMITES
    elemento.addEventListener("mousedown", (e) => {
        if ([rotateHandle, resizeHandle, deleteHandle].includes(e.target)) return;
        isDragging = true;
        const rect = elemento.getBoundingClientRect();
        offsetX = e.clientX - rect.left;
        offsetY = e.clientY - rect.top;
        elemento.style.zIndex = 1000;
    });

    document.addEventListener("mousemove", (e) => {
        if (isDragging) {
            const parent = elemento.parentElement.getBoundingClientRect();
            const limites = getLimites();

            let x = e.clientX - parent.left - offsetX;
            let y = e.clientY - parent.top - offsetY;

            // 🎯 RESTRINGIR AL ÁREA DE DISEÑO
            if (limites) {
                x = Math.max(limites.minX, Math.min(x, limites.maxX - elemento.offsetWidth));
                y = Math.max(limites.minY, Math.min(y, limites.maxY - elemento.offsetHeight));
            }

            elemento.style.left = x + "px";
            elemento.style.top = y + "px";
        }

        if (isResizing) {
            const dx = e.clientX - startX;
            const dy = e.clientY - startY;
            elemento.style.width = startWidth + dx + "px";
            elemento.style.height = startHeight + dy + "px";
        }

        if (isRotating) {
            const rect = elemento.getBoundingClientRect();
            const cx = rect.left + rect.width / 2;
            const cy = rect.top + rect.height / 2;
            const angle = Math.atan2(e.clientY - cy, e.clientX - cx);
            const deg = angle * (180 / Math.PI);
            elemento.style.transform = `rotate(${deg}deg)`;
        }
    });

    document.addEventListener("mouseup", () => {
        isDragging = false;
        isResizing = false;
        isRotating = false;
    });

    resizeHandle.addEventListener("mousedown", (e) => {
        e.stopPropagation();
        isResizing = true;
        startX = e.clientX;
        startY = e.clientY;
        startWidth = elemento.offsetWidth;
        startHeight = elemento.offsetHeight;
    });

    rotateHandle.addEventListener("mousedown", (e) => {
        e.stopPropagation();
        isRotating = true;
    });

    deleteHandle.addEventListener("click", (e) => {
        e.stopPropagation();
        elemento.remove();
    });

    elemento.addEventListener("mouseenter", () => {
        controlsContainer.style.display = "block";
        elemento.style.outline = "1px dashed #555";
    });

    elemento.addEventListener("mouseleave", () => {
        controlsContainer.style.display = "none";
        elemento.style.outline = "none";
    });

    controlsContainer.style.display = "none";
}



// ==========================================================
//  BOTONES INFERIORES (ZOOM, DESHACER, ETC.)
// ==========================================================
function initializeBottomTools() {
    // Zoom
    let zoomLevel = 1;  // nivel inicial de zoom
    const zoomBtn = document.getElementById("btnZoom"); // botón de zoom
    const designArea = document.querySelector(".design-area");

    if (zoomBtn && designArea) {
        zoomBtn.addEventListener("click", () => {
            zoomLevel += 0.2; // aumentar zoom
            if (zoomLevel > 1.6) zoomLevel = 1;// reiniciar si pasa de 160%
            designArea.style.transform = `scale(${zoomLevel})`;
            designArea.style.transition = "transform 0.4s ease";
        });
    }

    // Deshacer
    const undoBtn = document.getElementById("btnUndo");
    if (undoBtn) {
        undoBtn.addEventListener("click", () => {
            console.log("🔙 Deshacer acción");
        });
    }

    // Rehacer
    const redoBtn = document.getElementById("btnRedo");
    if (redoBtn) {
        redoBtn.addEventListener("click", () => {
            console.log("🔜 Rehacer acción");
        });
    }

    // Vista previa
    const previewBtn = document.getElementById("btnPreview");
    if (previewBtn) {
        previewBtn.addEventListener("click", mostrarVistaPrevia);
    }

    // Seleccionar todo
    const selectAllBtn = document.getElementById("btnSelectAll");
    if (selectAllBtn) {
        selectAllBtn.addEventListener("click", () => {
            document.querySelectorAll(".arrastrable-escalable").forEach(el => {
                el.style.outline = "2px dashed #3498db";
            });
            console.log("🖱️ Todos los elementos seleccionados");
        });
    }
}



// ==========================================================
//  VISTA PREVIA 
// ==========================================================
function mostrarVistaPrevia() {
    const frontView = document.getElementById('front-view');
    const backView = document.getElementById('back-view');

    if (!frontView || !backView) {
        alert("Error: no se encontraron las vistas del diseño.");
        return;
    }

    // Crear modal primero
    const modal = document.createElement("div");
    modal.className = "preview-modal";
    modal.style.cssText = `
        position: fixed;
        inset: 0;
        background: rgba(0, 0, 0, 0.9);
        display: flex;
        justify-content: center;
        align-items: center;
        z-index: 10000;
        padding: 20px;
        overflow-y: auto;
    `;

    modal.innerHTML = `
        <div style="
            background: white;
            border-radius: 12px;
            padding: 30px;
            max-width: 1400px;
            width: 95%;
            box-shadow: 0 0 40px rgba(0,0,0,0.5);
            position: relative;
        ">
            <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 30px; border-bottom: 2px solid #c89f2a; padding-bottom: 15px;">
                <h2 style="color: #1a1a1a; font-size: 1.8rem; margin: 0;">Vista previa del diseño</h2>
                <span class="close-preview" style="
                    font-size: 32px;
                    cursor: pointer;
                    color: #666;
                    transition: color 0.3s;
                    line-height: 1;
                    font-weight: bold;
                ">&times;</span>
            </div>

            <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 40px;">
                <div style="text-align: center;">
                    <h3 style="margin-bottom: 20px; color: #1a1a1a; font-size: 1.3rem; font-weight: 600;">Vista Frontal</h3>
                    <div class="preview-container-front" style="
                        background: #f9f9f9; 
                        border-radius: 12px; 
                        padding: 20px; 
                        display: flex; 
                        justify-content: center; 
                        align-items: center;
                        min-height: 500px;
                        border: 2px solid #e0e0e0;
                        position: relative;
                        overflow: hidden;
                    ">
                    </div>
                </div>
                <div style="text-align: center;">
                    <h3 style="margin-bottom: 20px; color: #1a1a1a; font-size: 1.3rem; font-weight: 600;">Vista Trasera</h3>
                    <div class="preview-container-back" style="
                        background: #f9f9f9; 
                        border-radius: 12px; 
                        padding: 20px; 
                        display: flex; 
                        justify-content: center; 
                        align-items: center;
                        min-height: 500px;
                        border: 2px solid #e0e0e0;
                        position: relative;
                        overflow: hidden;
                    ">
                    </div>
                </div>
            </div>

            <div style="
                margin-top: 25px; 
                text-align: center; 
                color: #666; 
                font-size: 14px;
                padding-top: 20px;
                border-top: 1px solid #e0e0e0;
            ">
                <p style="margin: 0;">✨ Esta es una vista previa de cómo se verá tu diseño personalizado</p>
            </div>
        </div>
    `;

    document.body.appendChild(modal);

    const frontDesign = frontView.querySelector('.product-design');
    const backDesign = backView.querySelector('.product-design');

    if (frontDesign && backDesign) {
        const frontClone = frontDesign.cloneNode(true);
        const backClone = backDesign.cloneNode(true);

        [frontClone, backClone].forEach(clone => {
            clone.style.cssText = `
                position: relative;
                width: 400px;
                height: 480px;
                display: block;
                margin: 0 auto;
            `;

            // Asegurar que la imagen de la camisa sea visible
            const shirtImg = clone.querySelector('.modelImage');
            if (shirtImg) {
                shirtImg.style.cssText = `
                    position: absolute;
                    top: 0;
                    left: 0;
                    width: 100%;
                    height: 100%;
                    object-fit: contain;
                    display: block !important;
                    opacity: 1 !important;
                    visibility: visible !important;
                    z-index: 1;
                `;
            }

            // Mantener el overlay visible
            const overlay = clone.querySelector('.design-area-overlay');
            if (overlay) {
                overlay.style.cssText = `
                    position: absolute;
                    top: 50%;
                    left: 50%;
                    transform: translate(-50%, -50%);
                    width: 222px;
                    height: 310px;
                    z-index: 100;
                `;
            }

            clone.querySelectorAll('.arrastrable-escalable').forEach(el => {
                el.style.border = 'none';
                el.style.outline = 'none';
                el.style.cursor = 'default';
                el.style.pointerEvents = 'none';

                const controlsContainer = el.querySelector('div[style*="inset: 0"]');
                if (controlsContainer) {
                    controlsContainer.remove();
                }
            });
        });

        // Insertar en los contenedores
        const containerFront = modal.querySelector('.preview-container-front');
        const containerBack = modal.querySelector('.preview-container-back');

        if (containerFront) containerFront.appendChild(frontClone);
        if (containerBack) containerBack.appendChild(backClone);
    }

    const closeBtn = modal.querySelector(".close-preview");
    if (closeBtn) {
        closeBtn.addEventListener("click", cerrarModal);
        closeBtn.addEventListener("mouseenter", () => closeBtn.style.color = "#c89f2a");
        closeBtn.addEventListener("mouseleave", () => closeBtn.style.color = "#666");
    }

    modal.addEventListener("click", (e) => {
        if (e.target === modal) cerrarModal();
    });

    const handleEscape = (e) => {
        if (e.key === 'Escape') {
            cerrarModal();
            document.removeEventListener('keydown', handleEscape);
        }
    };
    document.addEventListener('keydown', handleEscape);

    function cerrarModal() {
        modal.remove();
        document.body.style.overflow = 'auto';
    }

    document.body.style.overflow = 'hidden';
    console.log('✅ Vista previa generada correctamente');
}


// ==========================================================
//  MODAL DE CATEGORÍAS Y PRODUCTOS - MEJORADO
// ==========================================================
document.addEventListener('DOMContentLoaded', function () {
    const modalElement = document.getElementById('modalCategorias');
    const categoriasContainer = document.getElementById('categoriasContainer');
    const productosContainer = document.getElementById('productosContainer');
    const btnCerrarModal = document.getElementById('btnCerrarModalCategorias');

    if (!modalElement) return;

    const modalBootstrap = new bootstrap.Modal(modalElement, {
        backdrop: 'static',
        keyboard: false
    });

    // Verificar si ya hay un producto seleccionado en la URL
    const urlParams = new URLSearchParams(window.location.search);
    const productoId = urlParams.get('productoId');

    if (!productoId) {
        // 🔒 NO HAY PRODUCTO: Modal obligatorio sin X
        console.log("⚠️ No hay producto seleccionado - Modal obligatorio");
        modalBootstrap.show();
        btnCerrarModal.style.display = 'none'; // Ocultar botón X
    } else {
        // ✅ YA HAY PRODUCTO: Modal opcional con X visible
        console.log("🧩 Producto seleccionado:", productoId);
        btnCerrarModal.style.display = 'block'; // Mostrar botón X
    }

    // Cargar categorías
    fetch('/Categorias/ObtenerCategorias')
        .then(res => res.json())
        .then(categorias => {
            if (!categorias || categorias.length === 0) {
                categoriasContainer.innerHTML = `<p style="text-align:center; color:#777; margin:20px 0;">No hay categorías disponibles.</p>`;
                return;
            }

            // Mostrar categorías
            categoriasContainer.innerHTML = categorias.map(c => `
                <div class="categoria-card" data-id="${c.id}">
                    🧵 ${c.nombre}
                </div>
            `).join('');

            // Click en categoría
            document.querySelectorAll('.categoria-card').forEach(card => {
                card.addEventListener('click', function () {
                    const categoriaId = this.dataset.id;

                    fetch(`/Productos/ObtenerPorCategoria/${categoriaId}`)
                        .then(res => res.json())
                        .then(productos => {
                            productosContainer.innerHTML = '';
                            if (!productos || productos.length === 0) {
                                productosContainer.innerHTML = `<p style="text-align:center; color:#777; margin:20px 0;">No hay productos en esta categoría.</p>`;
                                return;
                            }

                            // Mostrar productos con control de stock
                            productos.forEach(p => {
                                const div = document.createElement('div');
                                div.className = 'producto-card';

                                const sinStock = p.stock <= 0;
                                const stockText = sinStock ? "Sin stock" : `Stock: ${p.stock}`;
                                const botonDeshabilitado = sinStock ? "disabled" : "";

                                div.innerHTML = `
                                    <img src="${p.imagenUrlFrende}" alt="${p.descripcion}">
                                    <p style="font-weight:bold; font-size:16px; margin-bottom:8px;">${p.descripcion}</p>
                                    <p style="color:#ff5722; font-weight:600; margin-bottom:5px;">₡${p.precio}</p>
                                    <p style="color:${sinStock ? '#c00' : '#4caf50'}; font-weight:600; margin-bottom:10px;">${stockText}</p>
                                    <button type="button" class="btn-aceptar" ${botonDeshabilitado}>${sinStock ? 'No disponible' : 'Aceptar'}</button>
                                `;

                                productosContainer.appendChild(div);

                                // Solo permitir clic si hay stock
                                if (!sinStock) {
                                    div.querySelector('.btn-aceptar').addEventListener('click', () => {
                                        console.log("Producto seleccionado:", p);
                                        window.location.href = `/Home/Diseno?productoId=${p.productoId}`;
                                    });
                                }
                            });
                        })
                        .catch(err => {
                            productosContainer.innerHTML = `<p style="color:red; text-align:center;">Error cargando productos.</p>`;
                            console.error(err);
                        });
                });
            });
        })
        .catch(err => {
            categoriasContainer.innerHTML = `<p style="color:red; text-align:center;">Error cargando categorías</p>`;
            console.error(err);
        });

    // 📂 Abrir modal desde el botón de categorías (cuando ya estás personalizando)
    const openCategorias = document.getElementById('openCategorias');
    if (openCategorias && modalElement) {
        openCategorias.addEventListener('click', () => {
            // ✅ Mostrar la X porque ya hay un producto seleccionado
            btnCerrarModal.style.display = 'block';
            modalBootstrap.show();
        });
    }
});

// === BOTÓN VOLVER AL INICIO ===
const btnInicio = document.getElementById("btnInicio");

// Acción: volver a la página principal
btnInicio.addEventListener("click", () => {
    window.location.href = "/Home/Index";
});

// Ocultar al pasar el mouse por los paneles laterales
const leftPanel = document.querySelector(".tools-panel");
const rightPanel = document.querySelector(".options-panel");

[leftPanel, rightPanel].forEach(panel => {
    if (!panel) return;
    panel.addEventListener("mouseenter", () => btnInicio.classList.remove("visible"));
    panel.addEventListener("mouseleave", () => btnInicio.classList.add("visible"));
});



// ==========================================================
//  CAPTURAR DISEÑO (FUNCIONA PARA FRENTE Y ESPALDA)
// ==========================================================
async function capturarDisenoActual(vista) {
    console.log(`🎨 Capturando diseño: ${vista.toUpperCase()}`);

    const area = vista === "frente"
        ? document.querySelector('#front-view .product-design')
        : document.querySelector('#back-view .product-design');

    if (!area) {
        console.error(`❌ No se encontró el área de diseño (${vista})`);
        return null;
    }

    try {
        if (typeof html2canvas === 'undefined') {
            console.error("❌ html2canvas no está cargado");
            return null;
        }

        const canvas = await html2canvas(area, {
            backgroundColor: null,
            useCORS: true,
            scale: 2,
            logging: false
        });

        const imagenBase64 = canvas.toDataURL("image/png");
        console.log(`✅ Captura completada (${vista}): ${imagenBase64.length} bytes`);
        return imagenBase64;

    } catch (error) {
        console.error(`❌ Error capturando ${vista}:`, error);
        return null;
    }
}

// ==========================================================
//  CAPTURAR AMBAS VISTAS (FRENTE Y ESPALDA) CON ESPERA
// ==========================================================
async function capturarAmbasVistas() {
    // Activar vista frontal
    document.getElementById('back-view').classList.remove('active');
    document.getElementById('front-view').classList.add('active');
    await new Promise(r => setTimeout(r, 100)); 
    const imagenFrente = await capturarDisenoActual("frente");

    // Activar vista trasera
    document.getElementById('front-view').classList.remove('active');
    document.getElementById('back-view').classList.add('active');
    await new Promise(r => setTimeout(r, 100)); 
    const imagenEspalda = await capturarDisenoActual("espalda");

    document.getElementById('back-view').classList.remove('active');
    document.getElementById('front-view').classList.add('active');

    return { imagenFrente, imagenEspalda };
}

// ==========================================================
//  MODAL DE INFORMACIÓN DEL PRODUCTO Y GUÍA DE TALLAS
// ==========================================================
function initializeInfoModal() {
    // ===== MODAL DE INFORMACIÓN DEL PRODUCTO =====
    const modalInfo = document.getElementById('modalInfo');
    const btnInfo = document.getElementById('btnInfo');
    const closeInfo = document.getElementById('closeInfo');

    if (modalInfo && btnInfo && closeInfo) {
        // Abrir modal de información
        btnInfo.addEventListener('click', (e) => {
            e.preventDefault(); // Evitar que el enlace recargue la página
            modalInfo.style.display = 'flex';
            document.body.style.overflow = 'hidden';
            console.log('✅ Modal de información abierto');
        });

        // Cerrar con la X
        closeInfo.addEventListener('click', () => {
            modalInfo.style.display = 'none';
            document.body.style.overflow = 'auto';
            console.log('✅ Modal de información cerrado');
        });

        // Cerrar al hacer clic fuera del contenido
        modalInfo.addEventListener('click', (e) => {
            if (e.target === modalInfo) {
                modalInfo.style.display = 'none';
                document.body.style.overflow = 'auto';
            }
        });

        console.log('✅ Modal de información del producto inicializado');
    } else {
        console.warn('⚠️ Modal de información o botón no encontrado');
    }

    // ===== MODAL DE GUÍA DE TALLAS =====
    const modalTallas = document.getElementById('modalTallas');
    const btnTallas = document.getElementById('btnTallas');
    const closeTallas = document.getElementById('closeTallas');

    if (modalTallas && btnTallas && closeTallas) {
        // Abrir modal de tallas
        btnTallas.addEventListener('click', (e) => {
            e.preventDefault(); // Evitar que el enlace recargue la página
            modalTallas.style.display = 'flex';
            document.body.style.overflow = 'hidden';
            console.log('✅ Modal de guía de tallas abierto');
        });

        // Cerrar con la X
        closeTallas.addEventListener('click', () => {
            modalTallas.style.display = 'none';
            document.body.style.overflow = 'auto';
            console.log('✅ Modal de guía de tallas cerrado');
        });

        // Cerrar al hacer clic fuera del contenido
        modalTallas.addEventListener('click', (e) => {
            if (e.target === modalTallas) {
                modalTallas.style.display = 'none';
                document.body.style.overflow = 'auto';
            }
        });

        console.log('✅ Modal de guía de tallas inicializado');
    } else {
        console.warn('⚠️ Modal de guía de tallas o botón no encontrado');
    }

    // ===== CERRAR AMBOS MODALES CON LA TECLA ESC =====
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape') {
            // Cerrar modal de información si está abierto
            if (modalInfo && modalInfo.style.display === 'flex') {
                modalInfo.style.display = 'none';
                document.body.style.overflow = 'auto';
            }
            // Cerrar modal de tallas si está abierto
            if (modalTallas && modalTallas.style.display === 'flex') {
                modalTallas.style.display = 'none';
                document.body.style.overflow = 'auto';
            }
        }
    });
}

window.capturarAmbasVistas = capturarAmbasVistas;

window.capturarDisenoActual = capturarDisenoActual;

window.addTextToDesign = addTextToDesign

window.hacerArrastrableYEscalable = hacerArrastrableYEscalable;

document.getElementById("btnReset").addEventListener("click", function () {
    window.location.href = "/Home/Index";
});
