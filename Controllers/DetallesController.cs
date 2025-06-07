using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AdventureWorks_POC.Models;
using AdventureWorks_POC.Models.Permisos;

namespace AdventureWorks_POC.Controllers
{
    [ValidarSession]
    public class DetallesController : Controller
    {
        private readonly SqlConnection _connection = new SqlConnection(Constans.cadena);
        // GET: Detalles
        public ActionResult Index(int Id)
        {
            if (TempData["Mensaje"] != null)
                ViewBag.Mensaje = TempData["Mensaje"];
            if (TempData["MensajeError"] != null)
                ViewBag.MensajeError = TempData["MensajeError"];

            Photo Photo = new Photo();

            using (SqlCommand cmd = new SqlCommand("obtPhotoById", _connection))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@PhotoID", Id);

                _connection.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        Photo = new Photo()
                        {
                            PhotoId = (int)reader["PhotoID"],
                            Title = reader["Title"].ToString(),
                            Description = reader["Description"].ToString(),
                            CreatedDate = (DateTime)reader["CreatedDate"],
                            Owner = reader["Owner"].ToString(),
                            PhotoFile = reader["PhotoFile"] as byte[],
                            OwnerName = reader["Name"].ToString(),
                        };

                        _connection.Close();
                    }
                }
                _connection.Close();
            }

            Photo.PhotoB64 = "data:image/jpeg;base64," + Convert.ToBase64String(Photo.PhotoFile);
            ViewBag.UserLogin = Session["user"];
            return View(Photo);
        }

        public ActionResult LoadComments(int PhotoId)
        {
            List<Comments> Comments = new List<Comments>();
            using (SqlCommand cmd = new SqlCommand("obtComments", _connection))
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@PhotoID", PhotoId);

                _connection.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Comments.Add(new Comments()
                        {
                            CommentId = (int)reader["CommentID"],
                            User = reader["User"].ToString(),
                            Subject = reader["Subject"].ToString(),
                            Body = reader["Body"].ToString(),
                            PhotoId = (int)reader["PhotoID"],
                            UserName = reader["Name"].ToString(),
                        });
                       
                    }
                }
                _connection.Close();
            }
            ViewBag.UserLogin = Session["user"];
            ViewBag.PhotoId = PhotoId;
            return View(Comments);
        }

        [HttpPost]
        public ActionResult SaveComment(Comments Comentario)
        {
            if (ModelState.IsValid)
            {
                User pSesion = Session["user"] as User;

                using (SqlCommand cmd = new SqlCommand("InsertComment", _connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Subject", Comentario.Subject);
                    cmd.Parameters.AddWithValue("@Body", Comentario.Body);
                    cmd.Parameters.AddWithValue("@User", pSesion.Username);
                    cmd.Parameters.AddWithValue("@PhotoID", Comentario.PhotoId);

                    _connection.Open();
                    cmd.ExecuteNonQuery();
                }

                TempData["Mensaje"] = "Comentario guardado correctamente.";
                return RedirectToAction("Index", new { Id = Comentario.PhotoId });
            }

            TempData["MensajeError"] = "Debes completar todos los campos.";
            return RedirectToAction("Index", new { Id = Comentario.PhotoId });
        }
    }
}
