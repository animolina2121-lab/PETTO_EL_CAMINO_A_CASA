
const API_URL = "http://localhost:5174/api/Usuarios";

const formulario = document.getElementById("formRegistro");
const mensaje = document.getElementById("mensajeRegistro");

formulario.addEventListener("submit", async (event) => {
    event.preventDefault();

    const nombre = document.getElementById("nombre").value.trim();
    const apellido = document.getElementById("apellido").value.trim();
    const dni = document.getElementById("dni").value.trim();
    const email = document.getElementById("email").value.trim();
    const telefono = document.getElementById("telefono").value.trim();
    const domicilio = document.getElementById("domicilio").value.trim();
    const password = document.getElementById("password").value;
    const confirmarPassword =
        document.getElementById("confirmarPassword").value;

    mensaje.className = "alert d-none";
    mensaje.textContent = "";

    if (password !== confirmarPassword) {
        mostrarMensaje("Las contraseñas no coinciden.", "danger");
        return;
    }

    if (password.length < 8) {
        mostrarMensaje(
            "La contraseña debe tener al menos 8 caracteres.",
            "danger"
        );
        return;
    }

    const usuario = {
        nombre,
        apellido,
        dni,
        email,
        telefono: telefono || null,
        domicilio: domicilio || null,
        contraseña: password
    };

    const boton = formulario.querySelector('button[type="submit"]');
    boton.disabled = true;
    boton.textContent = "Creando cuenta...";

    try {
        const respuesta = await fetch(API_URL, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(usuario)
        });

        const resultado = await respuesta.json();

        if (!respuesta.ok) {
            mostrarMensaje(
                resultado.mensaje || "No se pudo crear la cuenta.",
                "danger"
            );
            return;
        }

        mostrarMensaje(
            "¡Cuenta creada correctamente! Ya podés iniciar sesión.",
            "success"
        );

        formulario.reset();

    } catch (error) {
        console.error("Error al registrar usuario:", error);

        mostrarMensaje(
            "No se pudo conectar con el servidor. Verificá que el backend esté ejecutándose.",
            "danger"
        );
    } finally {
        boton.disabled = false;
        boton.textContent = "Crear mi cuenta";
    }
});

function mostrarMensaje(texto, tipo) {
    mensaje.textContent = texto;
    mensaje.className = `alert alert-${tipo}`;
}
