// ***********************************************************************
// Assembly         : ACBr.Net.Sat.ExtratoFr
// Author           : RFTD
// Created          : 06-28-2016
//
// Last Modified By : RFTD
// Last Modified On : 11-20-2017
// ***********************************************************************
// <copyright file="Class1.cs" company="ACBr.Net">
//		        		   The MIT License (MIT)
//	     		    Copyright (c) 2016 Grupo ACBr.Net
//
//	 Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//	 The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//	 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// </copyright>
// <summary></summary>
// ***********************************************************************

using ACBr.Net.Core.Extensions;
using ACBr.Net.Core.Logging;
using FastReport;
using FastReport.Export.Html;
using FastReport.Export.Pdf;
using System;
using System.IO;
using ACBr.Net.DFe.Core.Common;

namespace ACBr.Net.Sat.Extrato.FastReport
{
	public class ExtratoFastReport : ExtratoSat, IACBrLog
	{
		#region Fields

		private Report internalReport;

		#endregion Fields

		#region Events

		public event EventHandler<ExtratoEventArgs> OnGetExtrato;

		#endregion Events

		#region Propriedades

		public bool DescricaoUmaLinha { get; set; }

		public float EspacoFinal { get; set; }

		#endregion Propriedades

		#region Methods

    public override void ImprimirExtrato(CFe cfe)
    {
        PreparaExtrato(ExtratoLayOut.Completo, cfe);
        internalReport.RegisterData(new[] { cfe }, "CFe");
        Print();
    }

    public override void ImprimirExtratoResumido(CFe cfe)
    {
        PreparaExtrato(ExtratoLayOut.Resumido, cfe);
        internalReport.RegisterData(new[] { cfe }, "CFe");
        Print();
    }

    public override void ImprimirExtratoCancelamento(CFe cfe, CFeCanc cFeCanc)
    {
        PreparaExtrato(ExtratoLayOut.Cancelamento, cfe);
        internalReport.RegisterData(new[] { cfe }, "CFe");
        internalReport.RegisterData(new[] { cFeCanc }, "CFeCanc");
        Print();
    }

    private void Print()
    {
        internalReport.Prepare();

        switch (Filtro)
        {
            case ExtratoFiltro.Nenhum:
                if (MostrarPreview)
                    internalReport.Show();
                else
                    internalReport.Print();
                break;

            case ExtratoFiltro.PDF:
                var pdfExport = new PDFExport
                {
                    EmbeddingFonts = true,
                    ShowProgress = MostrarSetup,
                    PdfCompliance = PDFExport.PdfStandard.PdfX_3,
                    OpenAfterExport = MostrarPreview
                };

                internalReport.Export(pdfExport, NomeArquivo);
                break;

            case ExtratoFiltro.HTML:
                var htmlExport = new HTMLExport
                {
                    Format = HTMLExportFormat.MessageHTML,
                    EmbedPictures = true,
                    Preview = MostrarPreview,
                    ShowProgress = MostrarSetup
                };

                internalReport.Export(htmlExport, NomeArquivo);
                break;

            case ExtratoFiltro.Design:
                internalReport.Design();
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        internalReport.Dispose();
        internalReport = null;
    }

    private void PreparaExtrato(ExtratoLayOut tipo, CFe cfe)
    {
        internalReport = new Report();

        var e = new ExtratoEventArgs(tipo);
        OnGetExtrato.Raise(this, e);
        if (e.FilePath.IsEmpty() || !File.Exists(e.FilePath))
        {
            MemoryStream ms;
            switch (tipo)
            {
                case ExtratoLayOut.Completo:
                case ExtratoLayOut.Resumido:
                    ms = new MemoryStream(Properties.Resources.ExtratoSat);
                    break;

                case ExtratoLayOut.Cancelamento:
                    ms = new MemoryStream(Properties.Resources.ExtratoSatCancelamento);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(tipo), tipo, null);
            }

            internalReport.Load(ms);
        }
        else
        {
            internalReport.Load(e.FilePath);
        }

        internalReport.SetParameterValue("Logo", Logo.ToByteArray());
        internalReport.SetParameterValue("IsResumido", tipo == ExtratoLayOut.Resumido);
        internalReport.SetParameterValue("IsOneLine", DescricaoUmaLinha);
        internalReport.SetParameterValue("EspacoFinal", EspacoFinal);
        internalReport.SetParameterValue("Ambiente", (int?)cfe.InfCFe.Ide.TpAmb ?? 1);

        internalReport.PrintSettings.Copies = NumeroCopias;
        internalReport.PrintSettings.Printer = PrinterName;
        internalReport.PrintSettings.ShowDialog = MostrarSetup;
    }

    #endregion Methods
  }
}