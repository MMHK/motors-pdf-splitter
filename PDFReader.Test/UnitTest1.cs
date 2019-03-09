using System;
using System.IO;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using Xunit;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit.Abstractions;

namespace PDFReader.Test
{
    public class UnitTest1
    {
        protected const string PDFPath = @"C:\project\motors-pdf-splitter\PDFReader.Test\sample\combine-pdf-min.pdf";

        protected const string OutPath = @"C:\project\motors-pdf-splitter\PDFReader.Test\sample\ouput";
        
        protected readonly ITestOutputHelper output;

        public UnitTest1(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void TestSplitDocuments()
        {
            PdfDocument inputPDF = PdfReader.Open(PDFPath, PdfDocumentOpenMode.Import);

            var counter = inputPDF.Pages.Count;
            PdfDocument outputDocument = new PdfDocument();
            
            outputDocument.Version = inputPDF.Version;
            outputDocument.Info.Title = String.Format("Unkown-{0}", Guid.NewGuid());
            outputDocument.Info.Creator = inputPDF.Info.Creator;
            string Number = String.Empty;
            
            for (var i = 0; i < counter; i++)
            {
                var text = PDFHelper.ExtractText(inputPDF.Pages[i]);
                var combine = String.Join("", text);
                var tmpPcyNo = PDFHelper.MatchPolicyNumber(combine);

                if (Number == String.Empty && tmpPcyNo != null)
                {
                    outputDocument.AddPage(inputPDF.Pages[i]);
                    Number = tmpPcyNo;
                    continue;
                }
                
                if (!Number.Equals(tmpPcyNo))
                {
                    if (tmpPcyNo == null)
                    {
                        tmpPcyNo = String.Format("Unkown-{0}", Guid.NewGuid());
                    }
                    
                    outputDocument.Info.Title = Number;
                    outputDocument.Save(Path.Combine(OutPath, Number + ".pdf"));
                    outputDocument.Close();
                    
                    
                    Number = tmpPcyNo;
                    outputDocument = new PdfDocument();
                    outputDocument.Info.Title = Number;
                    outputDocument.Version = inputPDF.Version;
                    outputDocument.Info.Creator = inputPDF.Info.Creator;
                    
                    output.WriteLine(Number);
                }
                
                outputDocument.AddPage(inputPDF.Pages[i]);
            }

            if (outputDocument.Pages.Count > 0)
            {
                outputDocument.Info.Title = Number;
                outputDocument.Save(Path.Combine(OutPath, Number + ".pdf"));
                outputDocument.Close();
            }
        }
        
        [Fact]
        public void TestSplitDahSingRenewal()
        {
            using (var fs = File.Open(PDFPath, FileMode.Open))
            {
                var list = PDFHelper.SplitDahSingRenewal(fs);

                foreach (var item in list)
                {
                    output.WriteLine(item.Info.Title);
                }
            }
        }

        [Fact]
        public void TestZipPDFFiles()
        {
            using (var fs = File.Open(PDFPath, FileMode.Open))
            {
                var list = PDFHelper.SplitDahSingRenewal(fs);

                var output = PDFHelper.ZipPDFFiles(list);

                var savePath = PDFPath + ".zip";
                
                using (var fileStream = File.Create(savePath))
                {
                    output.CopyTo(fileStream);
                }
            }
        }


        [Fact]
        public void TestMatchPolicyNumber()
        {
            var src =
                "DAH SING INSURANCE Upon Renewal, policy minimum premium is revised to $1,200 + MIB/Levy and any loss/damage/legal liability to aircraft damage is excluded from this policy. EXPIRY NOTICE Insured/ Correspondence Address CHEUNG WING YEE VIVIAN 1/F NO. 18 LUEN SHING STREET FANLING N.T. Other Interested Party/ Hire Purchase/ Mortgagee Business/ Occupation Period of Insurance Renewal Premium Policy Class Private Motor Policy Number/ Endorsement Number 1156SSPMV18-0 Replace Policy No. Policy Issuing Date 27 December 2018 Banking and Finance 13 March 2018 00:00 to 12 March 2019 Gross Premium MIB Premium Payable Confirmation of Renewal (Policy No. 115655PMV18-0) D Please renew the Policy D Please renew the Policy with alternations as below Signature/Company Chop (if any) Date: HKD 1,200.00 HKD 36.00 HKD 1,236.00 To ensure that you are adequately insured, we recommend you to assess your current limit and review your insurance policies accordingly. If any material change is made whereby the risk of loss, damage or accident is increased, you should notify us immediately. The company reserves the right to review the terms and conditions of the Policy in the event of any loss occurring before the expiry date as stipulated above. For other details, exceptions, terms and conditions, please refer to original Policy Dah Sing Insurance Company (1976) Limited 20/F, Island Place Tower, 510 King's Road, North Point, Hong Kong T 852 2808 5000 F 852 2598 8008 DRIVER.COM.HK {INSURANCE AGENCY) LIMITED/% Page 1 of 3 ";
            var policyNumber = PDFHelper.MatchPolicyNumber(src);;
            if (policyNumber != null)
            {
                output.WriteLine(policyNumber);
            }
        }
    }
}