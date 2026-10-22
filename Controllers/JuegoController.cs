using Microsoft.AspNetCore.Mvc;
using RIK.Models;
using System.Text.Json;

namespace RIK.Controllers;

public class JuegoController : Controller
{
    BD bd = new BD();
    
    public void Teoria(string teoriaElegida)
    {
        List<CharlaTeoria> charla = bd.ObtenerCharlaTeoria(teoriaElegida);
        ViewBag.teoriaNivel = bd.TeoriaNivel(teoriaElegida);
        ViewBag.charla = charla;
        return View();
    }

   
    public IActionResult Nivel(int nivelElegido)
    {
        Nivel Nivel = new Nivel();
        ViewBag.NumeroNivel = bd.ObtenerNivel(nivelElegido);
        ViewBag.PreguntasNivel = Nivel.agarrarPreguntas(bd.ObtenerPreguntasNivel(nivelElegido));
        return View();
    }

     [HttpPost]
    public IActionResult ComprobarRespuesta(int idPregunta, string respuestaElegida)
    {
      int idPregunta;
      int opcionElegida;
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

        
        if(pregunta.Respuesta.ToString() == opcionElegida.ToString())
        {
            return Content("Correcta");
        }
        return Content("Incorrecta");
    }
    }



}
