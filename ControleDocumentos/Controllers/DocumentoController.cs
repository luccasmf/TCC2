﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ControleDocumentosLibrary;
using System.IO;
using ControleDocumentos.Repository;
using ControleDocumentos.Filter;

namespace ControleDocumentos.Controllers
{
   // [AuthorizeAD(Groups = "G_PROTOCOLO_ADMIN, G_FACULDADE_ALUNOS, G_FACULDADE_PROFESSOR_R, G_FACULDADE_PROFESSOR_RW")]
    public class DocumentoController : Controller
    {
        TipoDocumentoRepository tipoDocumentoRepository = new TipoDocumentoRepository();
        CursoRepository cursoRepository = new CursoRepository();
        AlunoRepository alunoRepository = new AlunoRepository();
        DocumentoRepository documentoRepository = new DocumentoRepository();


        // GET: Documento
        public ActionResult Index()
        {
            // apenas se decidirmos n usar o datatables como filtro
            // PopularDropDowns();

            return View(documentoRepository.GetAllDocs());
        }

        public ActionResult CadastrarDocumento(int? idDoc)
        {
            PopularDropDowns();
            //instancia model
            Documento doc = new Documento();

            if (idDoc.HasValue)
            {
                doc = documentoRepository.GetDocumentoById((int)idDoc);
                PopularDropDownAlunos(doc.AlunoCurso.Curso.IdCurso);
            }
            else
            {
                ViewBag.Alunos = new SelectList(new List<SelectListItem>() { new SelectListItem() {
                    Text ="Selecione um curso",
                    Value =""}
                }, "Value", "Text");
            }
            //retorna model
            return View("CadastroDocumento", doc);
        }

        public ActionResult List(Models.DocumentoModel filter)
        {
            // apenas caso decidirmos n usar o datatables como filtro

            var retorno = new List<Documento>();
            //busca os documentos com base no filtro

            return PartialView("_List", retorno);
        }

        public ActionResult CarregaModalExclusao(int idDoc)
        {
            Documento doc = documentoRepository.GetDocumentoById(idDoc);

            //retorna o tipo na partial
            return PartialView("_ExclusaoDocumento", doc);
        }

        #region Métodos auxiliares

        private void PopularDropDowns()
        {
            //get todos os cursos
            var listCursos = cursoRepository.GetCursos().Select(item => new SelectListItem
            {
                Value = item.IdCurso.ToString(),
                Text = item.Nome.ToString(),
            });
            ViewBag.Cursos = new SelectList(listCursos, "Value", "Text");


            var listTiposDoc = tipoDocumentoRepository.listaTipos().Select(item => new SelectListItem
            {
                Value = item.IdTipoDoc.ToString(),
                Text = item.TipoDocumento1.ToString(),
            });
            ViewBag.TiposDoc = new SelectList(listTiposDoc, "Value", "Text");

        }

        private void PopularDropDownAlunos(int idCurso) {
            //get todos alunos pelo id do curso
            var listAlunos = alunoRepository.GetAlunoByIdCurso(idCurso).Select(item => new SelectListItem
            {
                Value = item.IdAluno.ToString(),
                Text = item.Usuario.Nome.ToString(),
            });
            ViewBag.Alunos = new SelectList(listAlunos, "Value", "Text");
        }

        public object SalvarDocumento(Documento doc, HttpPostedFileBase uploadFile) //da pra negociarmos esse parametro
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (uploadFile == null)
                        return Json(new { Status = false, Type = "error", Message = "Selecione um documento" }, JsonRequestBehavior.AllowGet);

                    doc.arquivo = converterFileToArray(uploadFile);
                    doc.NomeDocumento = uploadFile.FileName;
                    string mensagem = DirDoc.SalvaArquivo(doc);

                    switch (mensagem)
                    {
                        case "Sucesso":
                            return Json(new { Status = true, Type = "success", Message = mensagem, ReturnUrl = Url.Action("Index") }, JsonRequestBehavior.AllowGet);
                        case "Falha ao persistir":
                            return Json(new { Status = false, Type = "error", Message = mensagem }, JsonRequestBehavior.AllowGet);
                        case "Falha ao criptografar":
                            return Json(new { Status = false, Type = "error", Message = mensagem }, JsonRequestBehavior.AllowGet);
                        default:
                            return null;
                    }
                }
                catch (Exception e)
                {
                    return Json(new { Status = false, Type = "error", Message = "Ocorreu um erro ao realizar esta operação" }, JsonRequestBehavior.AllowGet);
                }
            }
            else {
                return Json(new { Status = false, Type = "error", Message = "Campos inválidos" }, JsonRequestBehavior.AllowGet);
            }
        }

        public object ExcluirDocumento(Documento doc)
        {
            if (documentoRepository.DeletaArquivo(doc))
            {
                return Json(new { Status = true, Type = "success", Message = "Documento deletado com sucesso!", ReturnUrl = Url.Action("Index") }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Status = false, Type = "error", Message = "Ocorreu um erro ao realizar esta operação" }, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// Baixa arquivo
        /// </summary>
        /// <param name="doc"></param>
        /// <returns>retorna o arquivo pra download</returns>
        /// 
        public FileResult Download(string nomeDoc) //da pra vermos o melhor parametro
        {
            Documento doc = documentoRepository.GetDocumentoByNome(nomeDoc);

            string nomeArquivo = doc.NomeDocumento;
            string extensao = Path.GetExtension(nomeArquivo);

            string contentType = "application/" + extensao.Substring(1);

            byte[] bytes = DirDoc.BaixaArquivo(doc);

            return File(bytes, contentType, nomeArquivo);

        }

        public static byte[] converterFileToArray(HttpPostedFileBase x)
        {
            MemoryStream tg = new MemoryStream();
            x.InputStream.CopyTo(tg);
            byte[] data = tg.ToArray();

            return data;
        }

        public JsonResult GetAlunosByIdCurso(int idCurso)
        {
            if (idCurso > 0)
            {
                var lstAlunos = alunoRepository.GetAlunoByIdCurso(idCurso);
                return Json(lstAlunos.Select(x => new { Value = x.IdAluno, Text = x.Usuario.Nome }));
            }
            return Json(null);
        }
        #endregion
    }
}