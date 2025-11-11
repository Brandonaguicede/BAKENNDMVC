document.addEventListener('DOMContentLoaded', () => {
    const btnMostrarFormTarjeta = document.getElementById('btnMostrarFormTarjeta');
    const formNuevaTarjeta = document.getElementById('formNuevaTarjeta');
    const btnCancelarTarjeta = document.getElementById('btnCancelarTarjeta');
    const formTarjeta = document.getElementById('formTarjeta');
    const numeroTarjetaInput = document.getElementById('numeroTarjeta');
    const cvvInput = document.getElementById('cvv');
    const btnConfirmarPedido = document.getElementById('btnConfirmarPedido');

    // 1️⃣ MOSTRAR/OCULTAR FORMULARIO DE NUEVA TARJETA
    if (btnMostrarFormTarjeta) {
        btnMostrarFormTarjeta.addEventListener('click', () => {
            formNuevaTarjeta.style.display = 'block';
            btnMostrarFormTarjeta.style.display = 'none';
        });
    }

    if (btnCancelarTarjeta) {
        btnCancelarTarjeta.addEventListener('click', () => {
            formNuevaTarjeta.style.display = 'none';
            btnMostrarFormTarjeta.style.display = 'block';
            formTarjeta.reset();
        });
    }

    // 2️⃣ FORMATEAR NÚMERO DE TARJETA
    if (numeroTarjetaInput) {
        numeroTarjetaInput.addEventListener('input', function (e) {
            let value = e.target.value.replace(/\s/g, '').replace(/\D/g, '');
            let formattedValue = value.match(/.{1,4}/g)?.join(' ') || value;
            e.target.value = formattedValue;
        });
    }

    // 3️⃣ VALIDAR SOLO NÚMEROS EN CVV
    if (cvvInput) {
        cvvInput.addEventListener('input', function (e) {
            e.target.value = e.target.value.replace(/\D/g, '');
        });
    }

    // 4️⃣ AGREGAR NUEVA TARJETA
    if (formTarjeta) {
        formTarjeta.addEventListener('submit', async function (e) {
            e.preventDefault();

            const nombreTarjeta = document.getElementById('nombreTarjeta').value.trim();
            const numeroTarjeta = document.getElementById('numeroTarjeta').value.replace(/\s/g, '');
            const fechaExpiracion = document.getElementById('fechaExpiracion').value;
            const cvv = document.getElementById('cvv').value;

            // Validaciones
            if (!nombreTarjeta || !numeroTarjeta || !fechaExpiracion || !cvv) {
                alert('⚠️ Todos los campos son obligatorios');
                return;
            }

            if (numeroTarjeta.length < 13 || numeroTarjeta.length > 19) {
                alert('⚠️ Número de tarjeta inválido');
                return;
            }

            if (cvv.length < 3 || cvv.length > 4) {
                alert('⚠️ CVV inválido');
                return;
            }

            const [year, month] = fechaExpiracion.split('-');
            const expirationDate = new Date(year, month - 1, 1);

            let metodoTipo = 'Desconocido';
            if (/^4/.test(numeroTarjeta)) metodoTipo = 'Visa';
            else if (/^5[1-5]/.test(numeroTarjeta)) metodoTipo = 'Mastercard';
            else if (/^3[47]/.test(numeroTarjeta)) metodoTipo = 'American Express';
            else if (/^6/.test(numeroTarjeta)) metodoTipo = 'Discover';

            const tarjetaData = {
                Nombre: nombreTarjeta,
                Numero_Tarjeta: numeroTarjeta,
                ExpirationDate: expirationDate.toISOString(),
                CVV: cvv,
                Metodo_Tipo: metodoTipo
            };

            try {
                const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';

                const response = await fetch(window.appUrls.agregarTarjeta, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': token
                    },
                    body: JSON.stringify(tarjetaData)
                });

                const data = await response.json();

                if (data.success) {
                    alert('✅ ' + data.message);
                    agregarTarjetaALista(data.tarjeta);
                    formNuevaTarjeta.style.display = 'none';
                    btnMostrarFormTarjeta.style.display = 'block';
                    formTarjeta.reset();
                } else {
                    alert('❌ ' + data.message);
                }
            } catch {
                alert('Error al agregar la tarjeta');
            }
        });
    }

    // 5️⃣ AGREGAR TARJETA A LA LISTA VISUALMENTE
    function agregarTarjetaALista(tarjeta) {
        const tarjetasGuardadas = document.getElementById('tarjetas-guardadas');

        if (!tarjetasGuardadas.querySelector('h6')) {
            const titulo = document.createElement('h6');
            titulo.className = 'mb-3';
            titulo.textContent = 'Selecciona una tarjeta:';
            tarjetasGuardadas.insertBefore(titulo, tarjetasGuardadas.firstChild);
        }

        const mensajeVacio = tarjetasGuardadas.querySelector('.text-muted');
        if (mensajeVacio) mensajeVacio.remove();

        const tarjetaItem = document.createElement('div');
        tarjetaItem.className = 'tarjeta-item';
        tarjetaItem.innerHTML = `
            <input type="radio" 
                   name="tarjetaSeleccionada" 
                   id="tarjeta_${tarjeta.Metodo_De_PagoId}" 
                   value="${tarjeta.Metodo_De_PagoId}" 
                   checked>
            <label for="tarjeta_${tarjeta.Metodo_De_PagoId}">
                <i class="fas fa-credit-card"></i>
                <strong>${tarjeta.Metodo_Tipo}</strong> - ${tarjeta.Nombre}
                <br>
                <small>${tarjeta.UltimosDigitos}</small>
            </label>
        `;

        tarjetasGuardadas.appendChild(tarjetaItem);
    }

    // 5️⃣.5 ASIGNAR TARJETA AL CARRITO CUANDO SE SELECCIONA
    document.addEventListener('change', async function (e) {
        if (e.target && e.target.name === 'tarjetaSeleccionada') {
            const metodoPagoId = parseInt(e.target.value);

            if (metodoPagoId > 0) {
                try {
                    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';

                    const response = await fetch('/Carrito_De_Compra/AsignarMetodoPagoAlCarrito', {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                            'RequestVerificationToken': token
                        },
                        body: JSON.stringify({
                            metodoPagoId: metodoPagoId
                        })
                    });

                    await response.json();
                } catch {
                    // sin logs
                }
            }
        }
    });

    // 6️⃣ CONFIRMAR PEDIDO CON VALIDACIÓN DE TARJETA
    if (btnConfirmarPedido) {
        btnConfirmarPedido.addEventListener('click', async function () {
            const tarjetaSeleccionada = document.querySelector('input[name="tarjetaSeleccionada"]:checked');

            if (!tarjetaSeleccionada) {
                alert('⚠️ Debes seleccionar o agregar un método de pago');
                return;
            }

            if (!confirm('¿Confirmar tu pedido con esta tarjeta?')) return;

            this.disabled = true;
            this.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Procesando...';

            try {
                const carritoId = parseInt(this.getAttribute('data-carrito-id') || 0);
                const metodoPagoId = parseInt(tarjetaSeleccionada.value);
                const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';

                const response = await fetch(window.appUrls.confirmarPedido, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': token
                    },
                    body: JSON.stringify({
                        carritoId: carritoId,
                        metodoPagoId: metodoPagoId
                    })
                });



                const data = await response.json();

                if (data.success) {
                    alert(data.message);  // Muestra mensaje de compra exitosa
                    // Opcional: cambiar texto y dejar botón deshabilitado
                    this.innerHTML = '<i class="fas fa-check-circle"></i> Pedido confirmado';
                    this.disabled = true;
                } else {
                    alert('❌ Error: ' + data.message);
                    this.disabled = false;
                    this.innerHTML = '<i class="fas fa-check-circle"></i> Confirmar Pedido';
                }
            } catch {
                alert('Error al procesar el pedido');
                this.disabled = false;
                this.innerHTML = '<i class="fas fa-check-circle"></i> Confirmar Pedido';
            }
        });
    }


    // 7️⃣ VERIFICAR SESIÓN ACTIVA
    async function verificarSesionActiva() {
        try {
            const response = await fetch('/Pedido/VerificarSesion');
            await response.json();
        } catch {
            // sin logs
        }
    }

    verificarSesionActiva();
});
