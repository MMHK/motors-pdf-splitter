using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Content;
using PdfSharp.Pdf.Content.Objects;
using System.Linq;
using PdfSharp.Pdf.IO;

namespace PDFReader
{
    public static class PDFHelper
    {
        public static IEnumerable<string> ExtractText(this PdfPage page)
        {
            var content = ContentReader.ReadContent(page);
            var text = content.ExtractText();
            return text;
        }

        public static IEnumerable<string> ExtractText(this CObject cObject)
        {
            if (cObject is COperator)
            {
                var cOperator = cObject as COperator;
                if (cOperator.OpCode.Name == OpCodeName.Tj.ToString() ||
                    cOperator.OpCode.Name == OpCodeName.TJ.ToString())
                {
                    foreach (var cOperand in cOperator.Operands)
                    {
                        foreach (var txt in ExtractText(cOperand))
                            yield return txt;
                    }
                }

                if (cOperator.OpCode.Name == OpCodeName.ET.ToString())
                {
                    yield return "\r\n";
                }

                if (cOperator.OpCode.Name == OpCodeName.EI.ToString())
                {
                    yield return "\r\n";
                }

                if (cOperator.OpCode.Name == OpCodeName.Tm.ToString())
                {
//                    cOperator.Operands.
                }
            }
            else if (cObject is CSequence)
            {
                var cSequence = cObject as CSequence;
                foreach (var element in cSequence)
                foreach (var txt in ExtractText(element))
                    yield return txt;
            }
            else if (cObject is CString)
            {
                var cString = cObject as CString;
                yield return cString.Value;
            }
        }

        public static string MatchDahSingPolicyNumber(string src)
        {
            var config = new RegexConfig();
            config.Pattern = @"[0-9a-zA-Z]{6}(P|C)MV[0-9]{2}-[0-9]{1}";
            var reg = new Regex(config.Pattern, RegexOptions.IgnoreCase);
            var matches = reg.Matches(src);
            if (matches.Count > 0)
            {
                var match = matches.Cast<Match>().FirstOrDefault();
                if (match != null)
                {
                    return match.Value;
                }
            }

            return null;
        }
        
        public static string MatchZurichPolicyNumber(string src)
        {
            var config = new RegexConfig();
            config.Pattern = @"Z[A-Z]{2}[0-9a-zA-Z]{7,10}ZC";
            var reg = new Regex(config.Pattern, RegexOptions.IgnoreCase);
            var matches = reg.Matches(src);
            if (matches.Count > 0)
            {
                var match = matches.Cast<Match>().FirstOrDefault();
                if (match != null)
                {
                    return match.Value;
                }
            }

            return null;
        }

        public static IEnumerable<PdfDocument> SplitDahSingRenewal(Stream input)
        {
            PdfDocument inputPDF = PdfReader.Open(input, PdfDocumentOpenMode.Import);

            var counter = inputPDF.Pages.Count;
            PdfDocument outputDocument = new PdfDocument();
            
            outputDocument.Version = inputPDF.Version;
            outputDocument.Info.Creator = inputPDF.Info.Creator;
            string Number = null;
            
            for (var i = 0; i < counter; i++)
            {
                var text = ExtractText(inputPDF.Pages[i]);
                var combine = String.Join("", text);
                
                if (combine.Contains("EXPIRY NOTICE"))
                {
                    if (Number != null)
                    {
                        var flag = "-" + outputDocument.PageCount;
                        outputDocument.Info.Title = Number + flag;
                        yield return outputDocument;
                        
                        outputDocument = new PdfDocument();
                        outputDocument.Version = inputPDF.Version;
                        outputDocument.Info.Creator = inputPDF.Info.Creator;
                    }
                    Number = MatchDahSingPolicyNumber(combine);
                }
                
                outputDocument.AddPage(inputPDF.Pages[i]);
            }
            
            if (outputDocument.Pages.Count > 0)
            {
                var flag = "-" + outputDocument.PageCount;
                outputDocument.Info.Title = Number + flag;
                yield return outputDocument;
            }
            
            inputPDF.Close();
        }
        
        public static IEnumerable<PdfDocument> SplitZurichRenewal(Stream input)
        {
            PdfDocument inputPDF = PdfReader.Open(input, PdfDocumentOpenMode.Import);

            var counter = inputPDF.Pages.Count;
            PdfDocument outputDocument = new PdfDocument();
            
            outputDocument.Version = inputPDF.Version;
            outputDocument.Info.Creator = inputPDF.Info.Creator;
            string Number = null;
            
            for (var i = 0; i < counter; i++)
            {
                var text = ExtractText(inputPDF.Pages[i]);
                var combine = String.Join("", text).Replace("\r\n", "");
                
                if (combine.Contains("Private Car Insurance Renewal Notice"))
                {
                    if (Number != null)
                    {
                        var flag = "-" + outputDocument.PageCount;
                        outputDocument.Info.Title = Number + flag;
                        yield return outputDocument;
                        
                        outputDocument = new PdfDocument();
                        outputDocument.Version = inputPDF.Version;
                        outputDocument.Info.Creator = inputPDF.Info.Creator;
                    }
                    Number = MatchZurichPolicyNumber(combine);
                }
                
                outputDocument.AddPage(inputPDF.Pages[i]);
            }
            
            if (outputDocument.Pages.Count > 0)
            {
                var flag = "-" + outputDocument.PageCount;
                outputDocument.Info.Title = Number + flag;
                yield return outputDocument;
            }
            
            inputPDF.Close();
        }

        public static Stream ZipPDFFiles(IEnumerable<PdfDocument> list)
        {
            var output = new MemoryStream();
            using (var archive = new ZipArchive(output, ZipArchiveMode.Create, true))
            {
                foreach (var doc in list)
                {
                    var temp = new MemoryStream();
                    doc.Save(temp);
                    var entry = archive.CreateEntry(doc.Info.Title + ".pdf");
                    temp.Seek(0, SeekOrigin.Begin);
                    var tempZipStream = entry.Open();
                    temp.CopyTo(tempZipStream);
                    doc.Close();
                    temp.Close();
                    tempZipStream.Close();
                }
                archive.Dispose();
            }

            output.Seek(0, SeekOrigin.Begin);
            return output;
        }
    }
}