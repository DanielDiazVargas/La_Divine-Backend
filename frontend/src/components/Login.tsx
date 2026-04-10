import "../styles/login.css";

function Login() {
  return (
    <div className="login">
        {/* Ruta al logo en formato .svg PENDIENTE*/}
        <img src="/icons.svg" alt="Logo Teatro La Divine" className="logo" />
        
        <div className="login-container">
            <div className="login-header">
                Ingreso al sistema
            </div>
            <div className="login-body">
                
                <div className="input-group">
                    <div className="input-icon">👤</div>
                    <input type="email" className="input-field" placeholder="Correo Electrónico" id="email" required />
                </div>
                
                <div className="input-group">
                    <div className="input-icon">🔒</div>
                    <input type="password" className="input-field" placeholder="Contraseña" id="password" required />
                </div>

                <button className="btn-olvido">¿Olvidó su contraseña?</button>

                <button className="btn-ingresar" id="btn-login">Ingresar</button>
            </div>
        </div>
    </div>
  );
}

export default Login;