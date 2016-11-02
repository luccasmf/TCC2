﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ControleDocumentos.Repository;
using ControleDocumentosLibrary;
using ControleDocumentos.Util;

namespace ControleDocumentos.Controllers
{
    public class CursoController : BaseController
    {
        CursoRepository cursoRepository = new CursoRepository();
        // GET: Curso
        public ActionResult Index()
        {            
            return View(cursoRepository.GetCursos());
        }

        public object SalvaCurso(Curso curso) //serve pra cadastrar e editar
        {
            switch (cursoRepository.PersisteCurso(curso))
            {
                case "Cadastrado":
                    Utilidades.SalvaLog(Utilidades.UsuarioLogado, EnumAcao.Persistir, curso,null);
                    return Json(new { Status = true, Type = "success", Message = "Curso cadastrado com sucesso!", ReturnUrl = Url.Action("Index") }, JsonRequestBehavior.AllowGet);
                case "Alterado":
                    Utilidades.SalvaLog(Utilidades.UsuarioLogado, EnumAcao.Persistir, curso, curso.IdCurso);
                    return Json(new { Status = true, Type = "success", Message = "Curso alterado com sucesso!", ReturnUrl = Url.Action("Index") }, JsonRequestBehavior.AllowGet);
                case "Erro":
                    return Json(new { Status = false, Type = "error", Message = "" }, JsonRequestBehavior.AllowGet);
                default:
                    return Json(new { Status = false, Type = "error", Message = "" }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}