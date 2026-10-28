using Microsoft.AspNetCore.Mvc;
using RIK.Models;
using System.Text.Json;

namespace RIK.Controllers;

public class JuegoController : Controller
{
    BD bd = new BD();
    
    public IActionResult Teoria(string teoriaElegida)
    {
                if(!string.IsNullOrEmpty(HttpContext.Session.GetString("NombreUsuario")))
        {
            return RedirectToAction("IndexInicio", "Inicio");
        }
        else
        {
            List<CharlaTeoria> charla = bd.ObtenerCharlaTeoria(teoriaElegida);
            ViewBag.charla = charla;
            return View();
        }
    }

   
    public IActionResult Nivel(int nivelElegido)
    {
                if(!string.IsNullOrEmpty(HttpContext.Session.GetString("NombreUsuario")))
        {
            return RedirectToAction("IndexInicio", "Inicio");
        }
        else
        {
            Nivel Nivel = new Nivel();
            ViewBag.NumeroNivel = bd.ObtenerNivel(nivelElegido);
            ViewBag.PreguntasNivel = Nivel.agarrarPreguntas(bd.ObtenerPreguntasNivel(nivelElegido));
            return View();
        }
    }

     [HttpPost]
    public IActionResult ComprobarRespuesta(int idPregunta, string respuestaElegida)
    {
                if(!string.IsNullOrEmpty(HttpContext.Session.GetString("NombreUsuario")))
        {
            return RedirectToAction("IndexInicio", "Inicio");
        }
        else
        {
    
            string guardadas = HttpContext.Session.GetString(
                "PreguntasPartida"
            );
            if (guardadas == null)
            {
                return Content("PartidaInexistente");
            }
            List<int> ids = JsonSerializer.Deserialize<List<int>>(
                guardadas
            );

        if(ids == null || !ids.Contains(idPregunta))
        {
            return Content("PreguntaInvalida");
        }

        Pregunta pregunta = bd.ObtenerPregunta(idPregunta);
        if(pregunta == null)
        {
            return Content("PreguntaInvalida");
        }

        
        if(pregunta.Respuesta.ToString() == respuestaElegida.ToString())
        {
            return Content("Correcta");
        }
        return Content("Incorrecta");
    }



}
