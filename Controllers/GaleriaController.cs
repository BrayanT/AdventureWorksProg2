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
                    return GuardarFotoEnBD(title, description, bytesArchivo);
                }
            }
            TempData["MensajeError"] = "No se seleccionó ningún archivo válido.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult GuardarFotoEnBD(string title, string description, byte[] photoBytes)
        {
            if (ModelState.IsValid)
            {
                User pSesion = Session["user"] as User;
                int newPhotoId;

                using (SqlCommand cmd = new SqlCommand("InsertPhoto", _connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Title", title);
                    cmd.Parameters.AddWithValue("@Description", (object)description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@User", pSesion.Username);
                    cmd.Parameters.AddWithValue("@PhotoFile", photoBytes);

                    // toma el id del registro recien creado 
                    SqlParameter outputIdParam = new SqlParameter("@NewPhotoId", SqlDbType.Int){
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(outputIdParam);

                    _connection.Open();
                    cmd.ExecuteNonQuery();
                    newPhotoId = Convert.ToInt32(outputIdParam.Value);
                }
                TempData["Mensaje"] = "Se Subió la foto exitosamente.";
                return RedirectToAction("Index", "Detalles", new { Id = newPhotoId });
            }
            TempData["MensajeError"] = "Ocurrió un error al subir la foto.";
            return RedirectToAction("Index");
        }


    }
}