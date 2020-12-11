using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Mall.Web
{
    public class TemplateVisualizationView:IView
    {
        public string FileName { get; private set; }

        public string Path { get; }

        public TemplateVisualizationView(string fileName)
        {
            this.FileName = fileName;
        }
        /*
         public void Render(ViewContext viewContext, System.IO.TextWriter writer)
         {
             byte[] buffer;
             using(FileStream fs=new FileStream(this.FileName,FileMode.Open)){
                 buffer = new byte[fs.Length];
                 fs.Read(buffer, 0, buffer.Length);
             }
             writer.Write(Encoding.UTF8.GetString(buffer));
         }*/

        public Task RenderAsync(ViewContext context)
        {

            return null;
        }


        public Task RenderAsync(ViewContext viewContext, System.IO.TextWriter writer)
        {
            byte[] buffer;
            using (FileStream fs = new FileStream(this.FileName, FileMode.Open))
            {
                buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
            }
            writer.Write(Encoding.UTF8.GetString(buffer));

            return null;
        }


    }
}