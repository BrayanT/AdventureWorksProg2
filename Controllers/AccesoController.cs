using AdventureWorks_POC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Security.Cryptography;
using System.Data.SqlClient;
using System.Data;
using System.Web.Services.Description;

namespace AdventureWorks_POC.Controllers
{
    public class AccesoController : Controller
    {
        // este controller funcionara nada mas para los metodos de login y registro de usuarios nuevos
        
        //static string cadena = "Data Source=(local);Initial Catalog=AdventureWorks;Integrated Security=true";
        private readonly SqlConnection _connection = new SqlConnection(Constans.cadena);


        public ActionResult Login()
        {
            if (Session["user"] != null){
                // Ya hay sesión activa, redirige al home
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        public ActionResult Registrar()
        {
            if (Session["user"] != null){
                // Ya hay sesión activa, redirige al home
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public ActionResult Registrar(User oUser)
        {
            bool registrado;
            string mensaje;
            if(oUser.Password == oUser.ConfirmarClave)
            {
                oUser.Password = ConvertirSha256(oUser.Password);
            }
            else
            {
                ViewData["Mensaje"] = "Las contraseñas no coinciden";
                return View();
            }

            //using (SqlConnection cn = new SqlConnection(_connection)) { 
                SqlCommand sqlCommand = new SqlCommand("sp_RegistrarUser", _connection);
                sqlCommand.Parameters.AddWithValue("User", oUser.Username);
                sqlCommand.Parameters.AddWithValue("Name", oUser.Name);
                sqlCommand.Parameters.AddWithValue("Password", oUser.Password);
                sqlCommand.Parameters.Add("registrado",SqlDbType.Bit).Direction = ParameterDirection.Output;
                sqlCommand.Parameters.Add("mensaje", SqlDbType.VarChar,100).Direction = ParameterDirection.Output;
                sqlCommand.CommandType = CommandType.StoredProcedure;

                _connection.Open();
                sqlCommand.ExecuteNonQuery();

                registrado = Convert.ToBoolean(sqlCommand.Parameters["registrado"].Value);
                mensaje = sqlCommand.Parameters["mensaje"].Value.ToString();
            //}

            ViewData["Mensaje"] = mensaje;

            if (registrado){
                return RedirectToAction("Login", "Acceso" );
            }
            else{
                return View();
            }
        }

        [HttpPost]
        public ActionResult Login(User oUser)
        {
            oUser.Password = ConvertirSha256(oUser.Password);

            //using (SqlConnection cn = new SqlConnection(cadena)){
                SqlCommand sqlCommand = new SqlCommand("sp_ValidarUser", _connection);
                sqlCommand.Parameters.AddWithValue("User", oUser.Username);
                sqlCommand.Parameters.AddWithValue("Password", oUser.Password);
                sqlCommand.CommandType = CommandType.StoredProcedure;

                _connection.Open();
                oUser.Username = sqlCommand.ExecuteScalar().ToString();
           // }

            if(oUser.Username != "0"){
                Session["user"] = oUser;
                return RedirectToAction("Index", "Galeria");
            }
            else{
                ViewData["Mensaje"] = "Usuario no encontrado";
                return View();
            }
        }

        public static string ConvertirSha256(string texto){
            StringBuilder Sb = new StringBuilder();
            using (SHA256 hash = SHA256Managed.Create()) { 
                Encoding enc = Encoding.UTF8;
                byte[] result = hash.ComputeHash(enc.GetBytes(texto));

                foreach (byte b in result) { 
                    Sb.Append(b.ToString("x2"));
                }
            }
            return Sb.ToString();
        }


    }
}