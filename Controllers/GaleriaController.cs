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

    public class GaleriaController : Controller {

        private readonly SqlConnection _connection = new SqlConnection(Constans.cadena);

        // GET: Galeria
        public ActionResult Index()
        {
            if (TempData["Mensaje"] != null)
                ViewBag.Mensaje = TempData["Mensaje"];
            List<Photo> Posts = new List<Photo>();
            using (SqlCommand cmd = new SqlCommand("obtAllPosts", _connection))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;                

                _connection.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Posts.Add(new Photo()
                        {
                            PhotoId = (int)reader["PhotoID"],
                            Title = reader["Title"].ToString(),
                            PhotoFile = reader["PhotoFile"] as byte[],
                            Description = reader["Description"].ToString(),
                            CreatedDate = (DateTime)reader["CreatedDate"],
                            Owner = reader["Owner"].ToString(),
                            OwnerName = reader["Name"].ToString()
                        });                        
                    }
                }
                _connection.Close();
            }

            Posts.ForEach(x => x.PhotoB64 = $"data:image/jpeg;base64,{Convert.ToBase64String(x.PhotoFile)}");            
            ViewBag.UserLogin = Session["user"];
            return View(Posts);
        }

        [ValidarSession]
        public ActionResult SubirFoto() {

            return View();
        }

        [ValidarSession]
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

        [ValidarSession]
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