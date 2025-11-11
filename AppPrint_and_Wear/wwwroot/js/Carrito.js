// ==========================================================
//  CARRITO SIMPLIFICADO - DOBLE VISTA DE CAMISA (FRENTE Y ESPALDA)
// ==========================================================
class ShoppingCart {
    constructor() {
        this.items = [];
        this.carritoId = null;
        this.loadFromBackend();
    }

    getAntiForgeryToken() {
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenInput ? tokenInput.value : '';
    }

    // Cargar carrito desde backend
    async loadFromBackend() {
        try {
            const response = await fetch("/Carrito_De_Compra/ObtenerCarrito");
            const data = await response.json();

            if (!data.success || !data.items || data.items.length === 0) {
                this.items = [];
                this.carritoId = data.carritoId || null;
                this.render();
                return;
            }

            this.carritoId = data.carritoId;

            this.items = data.items.map(i => {
                const producto = i.Producto || i.producto || i.Productos || i.productos || {};
                return {
                    id: i.CartItemId || i.cartItemId,
                    name: producto.Descripcion || producto.descripcion || "Producto sin nombre",
                    price: parseFloat(producto.Precio ?? producto.precio ?? 0) || 0,
                    quantity: i.Cantidad || i.cantidad || 1,
                    talla: i.Talla || i.talla || "Sin talla",
                    stock: producto.Stock ?? producto.stock ?? 0,
                    imageFront: i.ImagenPersonalizadaFrente || i.imagenPersonalizadaFrente ||
                        producto.ImagenUrlFrente || producto.imagenUrlFrente || "/images/default-product.png",
                    imageBack: i.ImagenPersonalizadaEspalda || i.imagenPersonalizadaEspalda ||
                        producto.ImagenUrlEspalda || producto.imagenUrlEspalda || "/images/default-product.png"
                };
            });

            this.render();
        } catch {
            this.items = [];
            this.render();
        }
    }

    // Agregar producto al carrito desde personalización
    async addToCartFromCustomization() {
        const productoId = parseInt(document.querySelector("#productoId")?.value || "0");
        if (productoId === 0) {
            alert("⚠️ Error: No se encontró el ID del producto.");
            return;
        }

        const talla = window.getTallaSeleccionada ? window.getTallaSeleccionada() : null;
        if (!talla) {
            alert("⚠️ Por favor selecciona una talla antes de agregar al carrito.");
            return;
        }

        if (typeof capturarAmbasVistas !== 'function') {
            alert("❌ La función capturarAmbasVistas() no está definida.");
            return;
        }

        const { imagenFrente, imagenEspalda } = await capturarAmbasVistas();

        const itemParaEnviar = {
            ProductoId: productoId,
            Cantidad: 1,
            Talla: talla,
            ImagenPersonalizadaFrente: imagenFrente || null,
            ImagenPersonalizadaEspalda: imagenEspalda || null
        };

        try {
            const response = await fetch("/Carrito_De_Compra/AgregarDesdeFrontend", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "RequestVerificationToken": this.getAntiForgeryToken()
                },
                body: JSON.stringify(itemParaEnviar)
            });

            const data = await response.json();
            if (!data.success) throw new Error(data.message || "Error al guardar en backend");

            window.location.href = "/Carrito_De_Compra/Carrito";
        } catch (error) {
            alert("No se pudo agregar el producto al carrito.\n\n" + error.message);
        }
    }

    async removeItem(index) {
        const item = this.items[index];
        if (!item) return;

        try {
            const token = this.getAntiForgeryToken();
            const response = await fetch('/Carrito_De_Compra/EliminarDesdeFrontend', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify(item.id)
            });

            const data = await response.json();
            if (!data.success) throw new Error(data.message || 'Error al eliminar en backend');

            this.items.splice(index, 1);
            this.render();

            if (typeof actualizarCantidadCarrito === "function") {
                actualizarCantidadCarrito();
            }
        } catch (error) {
            alert('No se pudo eliminar el producto: ' + error.message);
        }
    }

    // Vaciar todo el carrito
    async clearCart() {
        try {
            const response = await fetch('/Carrito_De_Compra/VaciarCarrito', {
                method: 'POST',
                headers: { 'RequestVerificationToken': this.getAntiForgeryToken() }
            });
            const data = await response.json();
            if (!data.success) throw new Error(data.message || 'Error al vaciar el carrito');

            this.items = [];
            this.render();
        } catch (error) {
            alert('No se pudo vaciar el carrito. ' + error.message);
        }
    }


    async updateQuantity(index, quantity) {
        if (quantity <= 0) {
            await this.removeItem(index);
            return;
        }

        const item = this.items[index];
        if (!item) return;

        try {
            const token = this.getAntiForgeryToken();
            const response = await fetch('/Carrito_De_Compra/ActualizarCantidadItem', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify({
                    CartItemId: item.id,
                    Cantidad: quantity
                })
            });

            const data = await response.json();
            if (!data.success) throw new Error(data.message || 'Error al actualizar cantidad en backend');

            item.quantity = quantity;
            this.render();
        } catch (error) {
            alert('No se pudo actualizar la cantidad: ' + error.message);
        }
    }

    getSubtotal() {
        return this.items.reduce((total, i) => total + (i.price * i.quantity), 0);
    }

    getShippingCost() {
        const subtotal = this.getSubtotal();
        return subtotal >= 25000 ? 0 : 2500;
    }

    getIVA() {
        const subtotal = this.getSubtotal();
        return subtotal * 0.13;
    }

    render() {
        const container = document.getElementById('cart-items');
        const totalContainer = document.getElementById('totalContainer');

        if (!container) return;

        if (this.items.length === 0) {
            container.innerHTML = `
                <div class="alert alert-info text-center">
                    🛒 Tu carrito está vacío
                </div>`;
            if (totalContainer) totalContainer.style.display = 'none';
            return;
        }

        container.innerHTML = this.items.map((item, idx) => {
            const imgFront = item.designFront || item.imageFront;
            const imgBack = item.designBack || item.imageBack;

            return `
            <div class="card mb-3 p-3 shadow-sm">
                <div class="row g-0 align-items-center">
                    <div class="col-md-4 text-center">
                        <div class="d-flex justify-content-around">
                            <div>
                                <small class="text-muted d-block mb-1">Frente</small>
                                <img src="${imgFront}" 
                                     alt="Frente" 
                                     class="img-fluid rounded border"
                                     style="max-width: 150px; max-height: 200px; object-fit: contain;"
                                     onerror="this.src='/images/default-product.png'">
                            </div>
                            <div>
                                <small class="text-muted d-block mb-1">Espalda</small>
                                <img src="${imgBack}" 
                                     alt="Espalda" 
                                     class="img-fluid rounded border"
                                     style="max-width: 150px; max-height: 200px; object-fit: contain;"
                                     onerror="this.src='/images/default-product.png'">
                            </div>
                        </div>
                    </div>
                    <div class="col-md-5">
                        <h5 class="mb-2">${item.name}</h5>
                        <p class="text-muted mb-1"><i class="fas fa-tag"></i> Precio: ₡${Number(item.price).toLocaleString()}</p>
                        <p class="text-muted mb-1"><i class="fas fa-tshirt"></i> Talla: <strong>${item.talla}</strong></p>
                        <p class="mb-0"><i class="fas fa-box"></i> Cantidad: ${item.quantity}</p>
                    </div>
                    <div class="col-md-3 text-end">
                        <div class="btn-group mb-3" role="group">
                            <button class="btn btn-sm btn-outline-secondary"
                                    onclick="cart.updateQuantity(${idx}, ${item.quantity - 1})"
                                    ${item.quantity <= 1 ? 'disabled' : ''}>
                                <i class="fas fa-minus"></i>
                            </button>
                            <button class="btn btn-sm btn-outline-secondary disabled" style="min-width: 50px;">
                                ${item.quantity}
                            </button>
                            <button class="btn btn-sm btn-outline-secondary"
                                    onclick="cart.updateQuantity(${idx}, ${item.quantity + 1})"
                                    ${item.quantity >= item.stock ? 'disabled' : ''}>
                                <i class="fas fa-plus"></i>
                            </button>
                        </div>
                        <button class="btn btn-sm btn-danger mb-3" onclick="cart.removeItem(${idx})">
                            <i class="fas fa-trash"></i> Eliminar
                        </button>
                        <div class="alert alert-success mb-0 py-2">
                            <strong>Subtotal:</strong><br>
                            ₡${Number(item.price * item.quantity).toLocaleString()}
                        </div>
                    </div>
                </div>
            </div>`;
        }).join('');

        if (totalContainer) {
            const subtotalEl = document.getElementById('subtotal');
            const ivaEl = document.getElementById('iva');
            const shippingEl = document.getElementById('shipping');
            const totalEl = document.getElementById('total');

            const subtotal = this.getSubtotal();
            const iva = this.getIVA();
            const shipping = this.getShippingCost();
            const total = subtotal + iva + shipping;

            if (subtotalEl) subtotalEl.textContent = `₡${Math.round(subtotal).toLocaleString()}`;
            if (ivaEl) ivaEl.textContent = `₡${Math.round(iva).toLocaleString()}`;
            if (shippingEl) {
                if (shipping === 0) {
                    shippingEl.innerHTML = `<span style="color: #28a745;">₡0 (¡GRATIS!)</span>`;
                } else {
                    const faltante = 25000 - subtotal;
                    shippingEl.innerHTML = `₡${shipping.toLocaleString()} <small style="color: #666; display: block; margin-top: 4px;">Agrega ₡${faltante.toLocaleString()} más para envío gratis</small>`;
                }
            }
            if (totalEl) totalEl.textContent = `₡${Math.round(total).toLocaleString()}`;
            totalContainer.style.display = 'block';
        }
    }

    async proceedToCheckout() {
        if (this.items.length === 0) {
            alert('Tu carrito está vacío. Agrega productos antes de continuar.');
            return;
        }

        try {
            const response = await fetch('/Pedido/VerificarSesion');
            const data = await response.json();

            if (!data.autenticado && !data.estaLogueado) {
                mostrarModalLogin();
            } else {
                if (typeof ocultarBotonesFlotantes === "function") ocultarBotonesFlotantes();
                await this.loadFromBackend();
                window.location.href = '/Pedido/Pedido';
            }
        } catch {
            alert('Error al procesar la solicitud. Por favor intenta de nuevo.');
        }
    }
}

// ==========================================================
//  FUNCIONES AUXILIARES
// ==========================================================
function mostrarModalLogin() {
    const modalElement = document.getElementById('loginModal');
    if (!modalElement) crearModalLogin();
    const myModal = new bootstrap.Modal(document.getElementById('loginModal'));
    myModal.show();
}

function crearModalLogin() {
    const modalHTML = `
    <div class="modal fade" id="loginModal" tabindex="-1" aria-labelledby="loginModalLabel" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header bg-warning text-dark">
                    <h5 class="modal-title" id="loginModalLabel">
                        <i class="fas fa-exclamation-triangle"></i> Inicio de Sesión Requerido
                    </h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body text-center">
                    <i class="fas fa-user-lock fa-3x text-warning mb-3"></i>
                    <p class="lead">Para finalizar tu compra necesitas iniciar sesión o crear una cuenta.</p>
                    <p class="text-muted">¿Qué deseas hacer?</p>
                </div>
                <div class="modal-footer justify-content-center">
                    <button class="btn btn-primary" onclick="abrirModalLogin()">
                        <i class="fas fa-sign-in-alt"></i> Iniciar Sesión
                    </button>
                    <button class="btn btn-success" onclick="abrirModalRegistro()">
                        <i class="fas fa-user-plus"></i> Registrarme
                    </button>
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
                        Cancelar
                    </button>
                </div>
            </div>
        </div>
    </div>`;
    document.body.insertAdjacentHTML('beforeend', modalHTML);
}



// ==========================================================
//  INICIALIZACIÓN GLOBAL
// ==========================================================
window.cart = new ShoppingCart();

document.addEventListener('DOMContentLoaded', () => {
    const btnAddToCart = document.querySelector('.add-to-cart');
    if (btnAddToCart) {
        btnAddToCart.addEventListener('click', () => {
            cart.addToCartFromCustomization();
        });
    }
});


function mostrarModalLogin() {
    const modalElement = document.getElementById('loginModal');
    if (!modalElement) crearModalLogin();
    const myModal = new bootstrap.Modal(document.getElementById('loginModal'));
    myModal.show();
}

function crearModalLogin() {
    const modalHTML = `
    <div class="modal fade" id="loginModal" tabindex="-1" aria-labelledby="loginModalLabel" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header bg-warning text-dark">
                    <h5 class="modal-title" id="loginModalLabel">
                        <i class="fas fa-exclamation-triangle"></i> Inicio de Sesión Requerido
                    </h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body text-center">
                    <i class="fas fa-user-lock fa-3x text-warning mb-3"></i>
                    <p class="lead">Para finalizar tu compra necesitas iniciar sesión o crear una cuenta.</p>
                    <p class="text-muted">¿Qué deseas hacer?</p>
                </div>
                <div class="modal-footer justify-content-center">
                    <button class="btn btn-primary" onclick="abrirModalLoginDesdeBootstrap()">
                        <i class="fas fa-sign-in-alt"></i> Iniciar Sesión
                    </button>
                    <button class="btn btn-success" onclick="abrirModalRegistroDesdeBootstrap()">
                        <i class="fas fa-user-plus"></i> Registrarme
                    </button>
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
                        Cancelar
                    </button>
                </div>
            </div>
        </div>
    </div>`;
    document.body.insertAdjacentHTML('beforeend', modalHTML);
}

window.abrirModalLoginDesdeBootstrap = () => {
    // Cerrar el modal de Bootstrap primero
    const loginModalElement = document.getElementById('loginModal');
    if (loginModalElement) {
        const bsModal = bootstrap.Modal.getInstance(loginModalElement);
        if (bsModal) {
            bsModal.hide();
        }
    }

    // Esperar a que se cierre completamente antes de abrir el siguiente
    setTimeout(() => {
        abrirModalLogin();
    }, 300);
}

window.abrirModalRegistroDesdeBootstrap = () => {
    // Cerrar el modal de Bootstrap primero
    const loginModalElement = document.getElementById('loginModal');
    if (loginModalElement) {
        const bsModal = bootstrap.Modal.getInstance(loginModalElement);
        if (bsModal) {
            bsModal.hide();
        }
    }

    // Esperar a que se cierre completamente antes de abrir el siguiente
    setTimeout(() => {
        abrirModalRegistro();
    }, 300);
}

const modalLogin = document.getElementById('modal-login');
const modalRegistro = document.getElementById('modal-registro');

window.abrirModalLogin = () => {
    modalLogin.style.display = 'block';
    modalLogin.style.zIndex = '10000'; 
    modalRegistro.style.display = 'none';
}

window.abrirModalRegistro = () => {
    modalRegistro.style.display = 'block';
    modalRegistro.style.zIndex = '10000'; 
    modalLogin.style.display = 'none';
}

document.getElementById('cerrar-login').onclick = () => modalLogin.style.display = 'none';
document.getElementById('cerrar-registro').onclick = () => modalRegistro.style.display = 'none';

window.onclick = e => {
    if (e.target === modalLogin) modalLogin.style.display = 'none';
    if (e.target === modalRegistro) modalRegistro.style.display = 'none';
};

// ======= LOGIN  =======
document.getElementById('formLogin').addEventListener('submit', async e => {
    e.preventDefault();
    const form = e.target;
    const token = form.querySelector('input[name="__RequestVerificationToken"]').value;
    const data = Object.fromEntries(new FormData(form));
    const mensajeDiv = document.getElementById('mensaje-login');
    const btnSubmit = form.querySelector('button[type="submit"]');

    btnSubmit.disabled = true;
    btnSubmit.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Iniciando sesión...';
    mensajeDiv.className = 'text-info mb-2 text-center';
    mensajeDiv.innerText = 'Procesando...';

    try {
        const response = await fetch(form.action, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify(data)
        });

        const result = await response.json();

        if (result.success) {
            mensajeDiv.className = 'text-success mb-2 text-center';
            mensajeDiv.innerHTML = '<i class="fas fa-check-circle"></i> ¡Inicio de sesión exitoso! Redirigiendo...';
            btnSubmit.innerHTML = '<i class="fas fa-check"></i> ¡Listo!';

            if (typeof cart !== 'undefined' && typeof cart.loadFromBackend === 'function') {
                await cart.loadFromBackend();
            }

            setTimeout(() => {
                window.location.href = '/Pedido/Pedido';
            }, 1000);
        } else {
            mensajeDiv.className = 'text-danger mb-2 text-center';
            mensajeDiv.innerText = result.message || 'Credenciales incorrectas.';
            btnSubmit.disabled = false;
            btnSubmit.innerHTML = 'Ingresar';
        }
    } catch (error) {
        console.error('Error en login:', error);
        mensajeDiv.className = 'text-danger mb-2 text-center';
        mensajeDiv.innerText = 'Error de comunicación con el servidor.';
        btnSubmit.disabled = false;
        btnSubmit.innerHTML = 'Ingresar';
    }
});



// ======= REGISTRO CON REDIRECCIÓN =======
document.getElementById('formRegistro').addEventListener('submit', async e => {
    e.preventDefault();
    const form = e.target;
    const token = form.querySelector('input[name="__RequestVerificationToken"]').value;
    const data = Object.fromEntries(new FormData(form));
    const mensajeDiv = document.getElementById('mensaje-registro');
    const btnSubmit = form.querySelector('button[type="submit"]');

    btnSubmit.disabled = true;
    btnSubmit.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Creando cuenta...';
    mensajeDiv.className = 'text-info mb-2 text-center';
    mensajeDiv.innerText = 'Procesando registro...';

    try {
        const response = await fetch(form.action, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify(data)
        });

        const result = await response.json();

        if (result.success) {
            mensajeDiv.className = 'text-success mb-2 text-center';
            mensajeDiv.innerHTML = '<i class="fas fa-check-circle"></i> ¡Cuenta creada exitosamente! Redirigiendo...';
            btnSubmit.innerHTML = '<i class="fas fa-check"></i> ¡Listo!';

            if (typeof cart !== 'undefined' && typeof cart.loadFromBackend === 'function') {
                await cart.loadFromBackend();
            }

            setTimeout(() => {
                window.location.href = '/Pedido/Pedido';
            }, 1000);
        } else {
            mensajeDiv.className = 'text-danger mb-2 text-center';
            mensajeDiv.innerText = result.message || 'Error al registrar.';
            btnSubmit.disabled = false;
            btnSubmit.innerHTML = 'Crear cuenta';
        }
    } catch (error) {
        console.error('Error en registro:', error);
        mensajeDiv.className = 'text-danger mb-2 text-center';
        mensajeDiv.innerText = 'Error en la comunicación con el servidor.';
        btnSubmit.disabled = false;
        btnSubmit.innerHTML = 'Crear cuenta';
    }
});
