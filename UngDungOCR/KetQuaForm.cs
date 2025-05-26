using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace UngDungOCR
{
    public partial class KetQuaForm : Form
    {
        public KetQuaForm()
        {
            InitializeComponent();
            this.Load += async (s, e) => await Form2_Load(s, e); // Gán sự kiện Load

        }

        private async Task LoadOcrResultToWebViewAsync(string ocrText)
        {
            string htmlContent = ConvertOcrTextToHtml(ocrText);

            // Ghi ra file temp
            string tempHtmlPath = Path.Combine(Path.GetTempPath(), "tempOcrLatex.html");
            File.WriteAllText(tempHtmlPath, htmlContent, Encoding.UTF8);

            await webView21.EnsureCoreWebView2Async();

            webView21.Source = new Uri(tempHtmlPath);
        }

        private async Task Form2_Load(object sender, EventArgs e)
        {
            string ocrText = @"这是第一段中文。
Here is some English text.
Đây là đoạn tiếng Việt có công thức LaTeX inline: \( E = mc^2 \)
Và đoạn block LaTeX:
\[
  \sum_{n=1}^{\infty} \frac{1}{n^2} = \frac{\pi^2}{6}
\]
Kết thúc đoạn văn bản.";

            await LoadOcrResultToWebViewAsync(ocrText);
        }

        private string ConvertOcrTextToHtml(string ocrText)
        {
            var lines = ocrText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var sb = new StringBuilder();

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html><head><meta charset='UTF-8'>");
            sb.AppendLine("<style>");
            sb.AppendLine(".latex-container { position: relative; display: inline-block; cursor: pointer; }");
            sb.AppendLine(".copy-icon { position: absolute; top: -20px; right: -20px; width: 20px; height: 20px;");
            sb.AppendLine("background: url('data:image/svg+xml;utf8,<svg fill=\"%23000\" xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 24 24\"><path d=\"M16 1H4a2 2 0 0 0-2 2v14h2V3h12V1zm3 4H8a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h11a2 2 0 0 0 2-2V7a2 2 0 0 0-2-2zm0 16H8V7h11v14z\"/></svg>') no-repeat center center; background-size: contain; opacity: 0; transition: opacity 0.3s ease; }");
            sb.AppendLine(".latex-container:hover .copy-icon { opacity: 1; }");
            sb.AppendLine("</style>");
            sb.AppendLine("<script type='text/javascript' async src='https://cdn.jsdelivr.net/npm/mathjax@3/es5/tex-mml-chtml.js'></script>");
            sb.AppendLine("</head><body>");

            bool inBlockLatex = false;
            var blockLatexLines = new List<string>();

            foreach (var line in lines)
            {
                var trimmed = line.Trim();

                if (trimmed.StartsWith(@"\["))
                {
                    inBlockLatex = true;
                    blockLatexLines.Clear();
                    blockLatexLines.Add(trimmed.Substring(2)); // Bỏ "\["
                }
                else if (trimmed.EndsWith(@"\]") && inBlockLatex)
                {
                    blockLatexLines.Add(trimmed.Substring(0, trimmed.Length - 2)); // Bỏ "\]"
                    inBlockLatex = false;

                    string latex = string.Join("\n", blockLatexLines).Trim();
                    sb.AppendLine($@"<div class='latex-container' data-latex='{System.Net.WebUtility.HtmlEncode(latex)}'>");
                    sb.AppendLine($@"\[ {latex} \]");
                    sb.AppendLine("<span class='copy-icon' title='Copy LaTeX'></span>");
                    sb.AppendLine("</div>");
                }
                else if (inBlockLatex)
                {
                    blockLatexLines.Add(trimmed);
                }
                else if (trimmed.Contains(@"\(") && trimmed.Contains(@"\)"))
                {
                    int start = trimmed.IndexOf(@"\(");
                    int end = trimmed.IndexOf(@"\)", start);
                    if (end > start)
                    {
                        string before = trimmed.Substring(0, start);
                        string latex = trimmed.Substring(start + 2, end - start - 2);
                        string after = trimmed.Substring(end + 2);

                        sb.Append("<p>");
                        sb.Append(System.Net.WebUtility.HtmlEncode(before));
                        sb.Append($@"<span class='latex-container' data-latex='{System.Net.WebUtility.HtmlEncode(latex)}'>");
                        sb.Append($@"\( {latex} \)");
                        sb.Append(@"<span class='copy-icon' title='Copy LaTeX'></span>");
                        sb.Append("</span>");
                        sb.Append(System.Net.WebUtility.HtmlEncode(after));
                        sb.AppendLine("</p>");
                    }
                    else
                    {
                        sb.AppendLine($"<p>{System.Net.WebUtility.HtmlEncode(trimmed)}</p>");
                    }
                }
                else
                {
                    sb.AppendLine($"<p>{System.Net.WebUtility.HtmlEncode(trimmed)}</p>");
                }
            }

            sb.AppendLine(@"<script>
  document.querySelectorAll('.latex-container').forEach(container => {
    const icon = container.querySelector('.copy-icon');
    icon.addEventListener('click', (event) => {
      event.stopPropagation();
      const latexCode = container.getAttribute('data-latex');
      if (navigator.clipboard && navigator.clipboard.writeText) {
        navigator.clipboard.writeText(latexCode).then(() => {
          alert('LaTeX code copied: ' + latexCode);
        }).catch(() => {
          fallbackCopyTextToClipboard(latexCode);
        });
      } else {
        fallbackCopyTextToClipboard(latexCode);
      }
    });

    function fallbackCopyTextToClipboard(text) {
      const textArea = document.createElement('textarea');
      textArea.value = text;
      textArea.style.position = 'fixed';
      textArea.style.top = '0';
      textArea.style.left = '0';
      textArea.style.width = '2em';
      textArea.style.height = '2em';
      textArea.style.padding = '0';
      textArea.style.border = 'none';
      textArea.style.outline = 'none';
      textArea.style.boxShadow = 'none';
      textArea.style.background = 'transparent';
      document.body.appendChild(textArea);
      textArea.focus();
      textArea.select();

      try {
        const successful = document.execCommand('copy');
        if (successful) {
          alert('LaTeX code copied: ' + text);
        } else {
          alert('Copy failed, please copy manually.');
        }
      } catch (err) {
        alert('Copy error: ' + err);
      }

      document.body.removeChild(textArea);
    }
  });
</script>");

            sb.AppendLine("</body></html>");

            return sb.ToString();
        }

    }
}
