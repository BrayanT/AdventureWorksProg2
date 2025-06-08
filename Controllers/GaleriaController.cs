using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using AdventureWorks_POC.Models;
using AdventureWorks_POC.Models.Permisos;
using System.Data.SqlClient;
using System.Data;


namespace AdventureWorks_POC.Controllers
{
    [ValidarSession]
    public class GaleriaController : Controller {

        private readonly SqlConnection _connection = new SqlConnection(Constans.cadena);

        // GET: Galeria
        public ActionResult Index()
        {
            return View();
        }

        //
        public ActionResult SubirFoto() {

            return View();
        }

        [HttpPost]
        public ActionResult SubirFoto(string title, string description)
        {
            var archivos = Request.Files;
            if (archivos.Count > 0) {
                var archivo = archivos[0];

                if (archivo != null && archivo.ContentLength > 0) {
                    byte[] bytesArchivo;

                    using (var inputStream = archivo.InputStream)
                    using (var memoryStream = new MemoryStream()){
                        inputStream.CopyTo(memoryStream);
                        bytesArchivo = memoryStream.ToArray();
                    }
                    GuardarFotoEnBD(title, description, bytesArchivo);
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult GuardarFotoEnBD(string title, string description, byte[] photoBytes)
        {
            if (ModelState.IsValid)
            {
                User pSesion = Session["user"] as User;

                using (SqlCommand cmd = new SqlCommand("InsertPhoto", _connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Title", title);
                    cmd.Parameters.AddWithValue("@Description", (object)description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@User", pSesion.Username);
                    cmd.Parameters.AddWithValue("@PhotoFile", photoBytes);

                    _connection.Open();
                    cmd.ExecuteNonQuery();
                }
                TempData["Mensaje"] = "Se Subio la foto exitosamente.";
                return RedirectToAction("Index");
            }
            TempData["MensajeError"] = "Fallo.";
            return RedirectToAction("Index");
        }


    }
}