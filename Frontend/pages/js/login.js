const API_USUARIOS = "http://localhost:5174/api/Usuarios";

const formulario = document.getElementById("formLogin");
const mensaje = document.getElementById("mensajeLogin");

formulario.addEventListener("submit", async function (evento) {
    evento.preventDefault();

    const email = document.getElementById("email").value.trim();
    const contraseña = document.getElementById("password").value;
    const recordar = document.getElementById("recordarme").checked;
    const boton = formulario.querySelector('button[type="submit"]');

    mensaje.className = "alert d-none";
    mensaje.textContent = "";
    boton.disabled = true;
    boton.textContent = "Verificando...";

    try {
        const respuesta = await fetch(`${API_USUARIOS}/login`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                email: email,
                contraseña: contraseña
            })
        });

        const datos = await respuesta.json();

        if (!respuesta.ok) {
            throw new Error(
                datos.mensaje || "No se pudo iniciar sesión."
            );
        }

        // Limpiar posibles datos de una sesión anterior.
        localStorage.removeItem("pettoUsuario");
        sessionStorage.removeItem("pettoUsuario");

        // Guardar los datos básicos para navegar por PETTO.
        const almacenamiento = recordar
            ? localStorage
            : sessionStorage;

        almacenamiento.setItem(
            "pettoUsuario",
            JSON.stringify(datos.usuario)
        );

        mensaje.textContent = datos.mensaje;
        mensaje.className = "alert alert-success";

        // Entrar al panel principal.
        window.location.href = "dashboard.html";

    } catch (error) {
        mensaje.textContent =
            error.message === "Failed to fetch"
                ? "No se pudo conectar con PETTO. Verificá que la API esté funcionando."
                : error.message;

        mensaje.className = "alert alert-danger";
    } finally {
        boton.disabled = false;
        boton.textContent = "Iniciar sesión";
    }
});
