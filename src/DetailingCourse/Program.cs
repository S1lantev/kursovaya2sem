using System;
using System.Linq;
using System.Text;
using System.Reflection;
namespace DetailingCourse {
class Program {
 static int Main(string[] args) {
  Console.OutputEncoding=Encoding.UTF8; Console.InputEncoding=Encoding.UTF8;
  try {
   if(args.Length>0) {
    var runner=Assembly.GetExecutingAssembly().GetType("DetailingCourse.Evidence");
    if(runner!=null) return (int)runner.GetMethod("Run").Invoke(null,new object[]{args});
    Console.WriteLine("Средства проверки появятся после интеграции модулей."); return 0;
   }
   var store=new Store(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"data","studio.json"));
   if(store.Recovered) {Console.WriteLine("Данные прочитаны из резервной копии."); Menu.Pause();}
   var modules=Assembly.GetExecutingAssembly().GetTypes().Where(t=>typeof(IModule).IsAssignableFrom(t)&&!t.IsInterface)
    .Select(t=>(IModule)Activator.CreateInstance(t)).OrderBy(t=>t.Order).ToArray();
   string[] names=modules.Select(x=>x.Name).Concat(new[]{"Выход"}).ToArray();
   while(true) {int choice=Menu.Choose("АС БДС — Детейлинг-студия",names); if(choice<0||choice==modules.Length) break;
    try {modules[choice].Run(store);} catch(Exception e){Console.WriteLine(e.Message); Menu.Pause();}
   } return 0;
  } catch(Exception e) { Console.WriteLine("Ошибка: "+(e.InnerException??e).Message); return 1; }
 }
}
}
