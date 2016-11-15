﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ControleDocumentosLibrary;
using ControleDocumentos.Util;

namespace ControleDocumentos.Repository
{
    public class AlunoRepository
    {
        DocumentosModel db = new DocumentosModel();
        CursoRepository cursoRepository = new CursoRepository();

        public List<Aluno> GetAlunoByIdCurso(int idCurso)
        {
            List<Aluno> alunos = (from al in db.Aluno
                                  join ac in db.AlunoCurso on al.IdAluno equals ac.IdAluno
                                  join cu in db.Curso on ac.IdCurso equals cu.IdCurso
                                  where cu.IdCurso == idCurso
                                  select al).ToList();

            return alunos;
        }

        public Aluno GetAlunoById(int idAluno)
        {
            return db.Aluno.Find(idAluno);
        }
        
        public List<Aluno> GetAlunoByCursoId(int idCurso)
        {
            List<Aluno> alunos = (from al in db.Aluno
                                  join ac in db.AlunoCurso on al.IdAluno equals ac.IdAluno
                                  join c in db.Curso on ac.IdCurso equals c.IdCurso
                                  where c.IdCurso == idCurso
                                  select al
                                  ).ToList();

            return alunos;
        }
        
        public Aluno GetAlunoByIdUsuario(string idUsuario)
        {
            Aluno al = db.Aluno.Where(x => x.IdUsuario == idUsuario).FirstOrDefault();

            return al;
        }

        public void AdicionaHoras(int cargaHoraria, int idAluno, int idEvento)
        {
            int idCurso = 0;
            
            List<Curso> cr = (from evento in db.Evento
                              join curso in db.Curso on (evento.Curso.Select(x => x.IdCurso).FirstOrDefault()) equals curso.IdCurso
                              where evento.IdEvento == idEvento
                              select curso).ToList();

            List<Curso> cs = (from cursoc in db.Curso
                              join alc in db.AlunoCurso on cursoc.IdCurso equals alc.IdCurso
                              where alc.IdAluno == idAluno
                              select cursoc).ToList();

            Curso c = cr.Intersect(cs).FirstOrDefault();
            idCurso = c.IdCurso;
            AlunoCurso ac = db.AlunoCurso.Where(x => x.IdAluno == idAluno && x.IdCurso == idCurso).FirstOrDefault();
            ac.HoraCompleta += cargaHoraria;
            db.SaveChanges();
        }

        public bool AdicionaHoras(int cargaHoraria, int idSolicitacaoDocumento)
        {
            SolicitacaoDocumento sol = db.SolicitacaoDocumento.Find(idSolicitacaoDocumento);

            sol.AlunoCurso.HoraCompleta += cargaHoraria;

            return db.SaveChanges() > 0;

        }
    }
}