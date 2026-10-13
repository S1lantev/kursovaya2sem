using System;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
namespace DetailingCourse {
public static class Evidence {
 static List<string> results=new List<string>(); static int failures=0;
 static void Check(string name,Action action){try{action();results.Add("PASS "+name);}catch(Exception e){failures++;results.Add("FAIL "+name+": "+e.Message);}}
 static void Assert(bool ok){if(!ok)throw new Exception("Проверка не выполнена");}
 static void Reject(Action f){try{f();}catch(ArgumentException){return;}throw new Exception("Ожидался отказ");}
 static DateTime When(int hour){return new DateTime(2026,10,20,hour,0,0);}
 static Booking Book(Store s,int hour,string master){return BookingModule.Add(s,"Учебный клиент","А001АА","Полировка",master,When(hour),120);}
 public static int Run(string[] args) {
  if(args[0]=="--self-test")return Tests(args.Length>1?args[1]:".");
  if(args[0]=="--demo")return Demo(args.Length>1?args[1]:".");
  Console.WriteLine("Параметры: --self-test <каталог>, --demo <каталог>.");return 1;
 }
 static int Tests(string output) {
  Directory.CreateDirectory(output);string folder=Path.Combine(output,"test-data-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(folder);
  Func<Store> fresh=()=>new Store(Path.Combine(folder,Guid.NewGuid().ToString("N")+".json"));
  Check("BOOK01 Свободный интервал",()=>{var s=fresh();Assert(Book(s,10,"М1").Id==1&&s.Data.Bookings.Count==1);});
  Check("BOOK02 Пересечение интервалов",()=>{var s=fresh();Book(s,10,"М1");Reject(()=>Book(s,11,"М1"));Assert(s.Data.Bookings.Count==1);});
  Check("BOOK03 Соседние интервалы",()=>{var s=fresh();Book(s,10,"М1");Book(s,12,"М1");Assert(s.Data.Bookings.Count==2);});
  Check("BOOK04 Разные мастера",()=>{var s=fresh();Book(s,10,"М1");Book(s,10,"М2");Assert(s.Data.Bookings.Count==2);});
  Check("BOOK05 Отмена освобождает время",()=>{var s=fresh();var b=Book(s,10,"М1");BookingModule.Cancel(s,b.Id);Book(s,10,"М1");Assert(s.Data.Bookings.Count==2);});
  Check("BOOK06 Пустой клиент",()=>{var s=fresh();Reject(()=>BookingModule.Add(s," ","А","У","М",When(10),60));Assert(s.Data.Bookings.Count==0);});
  Check("BOOK07 Нулевая длительность",()=>{var s=fresh();Reject(()=>BookingModule.Add(s,"К","А","У","М",When(10),0));});
  Check("BOOK08 За пределами смены",()=>{var s=fresh();Reject(()=>Book(s,17,"М1"));Reject(()=>Book(s,9,"М1"));});
  Check("BOOK09 Завершение на границе 18:00",()=>{var s=fresh();var b=Book(s,16,"М1");Assert(b.Start.AddMinutes(b.Minutes)==When(18));});
  Check("BOOK10 Повторная отмена",()=>{var s=fresh();var b=Book(s,10,"М1");BookingModule.Cancel(s,b.Id);Reject(()=>BookingModule.Cancel(s,b.Id));});
  Check("WORK01 Планировано — в работе — завершено",()=>{var s=fresh();var b=Book(s,10,"М1");JobsModule.Transition(s,b.Id,"М1","В работе");JobsModule.Transition(s,b.Id,"М1","Завершено");Assert(b.Status=="Завершено");});
  Check("WORK02 Чужой мастер",()=>{var s=fresh();var b=Book(s,10,"М1");Reject(()=>JobsModule.Transition(s,b.Id,"М2","В работе"));Assert(b.Status=="Запланировано");});
  Check("WORK03 Отменённая работа",()=>{var s=fresh();var b=Book(s,10,"М1");BookingModule.Cancel(s,b.Id);Reject(()=>JobsModule.Transition(s,b.Id,"М1","Завершено"));Assert(b.Status=="Отменено");});
  Check("WORK04 Пропуск состояния в работе",()=>{var s=fresh();var b=Book(s,10,"М1");Reject(()=>JobsModule.Transition(s,b.Id,"М1","Завершено"));});
  Check("WORK05 Неизвестный ID",()=>{var s=fresh();Reject(()=>JobsModule.Transition(s,99,"М1","В работе"));});
  Check("WORK06 Повторный старт",()=>{var s=fresh();var b=Book(s,10,"М1");JobsModule.Transition(s,b.Id,"М1","В работе");Reject(()=>JobsModule.Transition(s,b.Id,"М1","В работе"));});
  Check("MAT01 Точный десятичный остаток",()=>{var s=fresh();var b=Book(s,10,"М1");var m=MaterialsModule.Add(s,"Паста",5m,1m);MaterialsModule.Consume(s,m.Id,b.Id,.2m,"O1");Assert(m.Stock==4.8m&&s.Data.Usages.Count==1);});
  Check("MAT02 Недостаточный остаток",()=>{var s=fresh();var b=Book(s,10,"М1");var m=MaterialsModule.Add(s,"Паста",.2m,0);Reject(()=>MaterialsModule.Consume(s,m.Id,b.Id,.3m,"O1"));Assert(m.Stock==.2m&&s.Data.Usages.Count==0);});
  Check("MAT03 Порог пополнения",()=>{var s=fresh();var b=Book(s,10,"М1");var m=MaterialsModule.Add(s,"Химия",1.2m,1m);MaterialsModule.Consume(s,m.Id,b.Id,.3m,"O1");Assert(m.Stock==.9m&&m.Reorder);});
  Check("MAT04 Повторная операция",()=>{var s=fresh();var b=Book(s,10,"М1");var m=MaterialsModule.Add(s,"Химия",5m,1m);MaterialsModule.Consume(s,m.Id,b.Id,.2m,"O1");Reject(()=>MaterialsModule.Consume(s,m.Id,b.Id,.2m,"O1"));Assert(m.Stock==4.8m&&s.Data.Usages.Count==1);});
  Check("MAT05 Нулевой и отрицательный расход",()=>{var s=fresh();var b=Book(s,10,"М1");var m=MaterialsModule.Add(s,"Химия",1m,0);Reject(()=>MaterialsModule.Consume(s,m.Id,b.Id,0,"O1"));Reject(()=>MaterialsModule.Consume(s,m.Id,b.Id,-1,"O2"));});
  Check("MAT06 Точное достижение нуля",()=>{var s=fresh();var b=Book(s,10,"М1");var m=MaterialsModule.Add(s,"Химия",.2m,0);MaterialsModule.Consume(s,m.Id,b.Id,.2m,"O1");Assert(m.Stock==0&&!m.Reorder);});
  Check("MAT07 Отрицательный начальный остаток",()=>{var s=fresh();Reject(()=>MaterialsModule.Add(s,"Химия",-1,0));});
  Check("MAT08 Расход на отменённую запись",()=>{var s=fresh();var b=Book(s,10,"М1");BookingModule.Cancel(s,b.Id);var m=MaterialsModule.Add(s,"Химия",1,0);Reject(()=>MaterialsModule.Consume(s,m.Id,b.Id,.2m,"O1"));Assert(m.Stock==1);});
  Check("MAT09 Дубликат названия материала",()=>{var s=fresh();MaterialsModule.Add(s,"Паста",1,0);Reject(()=>MaterialsModule.Add(s,"паста",1,0));});
  Check("DATA01 Сохранение кириллицы и связей",()=>{var s=fresh();var b=Book(s,10,"М1");var m=MaterialsModule.Add(s,"Паста",5,1);MaterialsModule.Consume(s,m.Id,b.Id,.2m,"O1");s.Save();var reload=new Store(s.PathName);Assert(reload.Data.Bookings[0].Client=="Учебный клиент"&&reload.Data.Materials[0].Stock==4.8m&&reload.Data.Usages[0].BookingId==b.Id);});
  Check("DATA02 Создание резервной копии",()=>{var s=fresh();Book(s,10,"М1");s.Save();Book(s,12,"М1");s.Save();Assert(File.Exists(s.PathName+".bak")&&new Store(s.PathName).Data.Bookings.Count==2);});
  Check("DATA03 Восстановление из копии",()=>{var s=fresh();Book(s,10,"М1");s.Save();Book(s,12,"М1");s.Save();File.WriteAllText(s.PathName,"broken");var r=new Store(s.PathName);Assert(r.Recovered&&r.Data.Bookings.Count==1);r.Save();Assert(new Store(s.PathName).Data.Bookings.Count==1);});
  Check("DATA04 Некорректная ссылка расхода",()=>{var s=fresh();s.Data.Usages.Add(new Usage{Operation="O1",BookingId=99,MaterialId=99,Quantity=1});try{s.Save();}catch(InvalidDataException){return;}throw new Exception("Неверная ссылка сохранена");});
  Check("MENU01 Переход вверх и вниз с циклом",()=>{Assert(Menu.Move(0,-1,4)==3&&Menu.Move(3,1,4)==0);});
  Check("MENU02 Три функциональных пункта",()=>{var n=typeof(Evidence).Assembly.GetTypes().Count(t=>typeof(IModule).IsAssignableFrom(t)&&!t.IsInterface);Assert(n==3);});
  foreach(var r in results)Console.WriteLine(r);
  string summary=String.Format("{0} passed, {1} failed",results.Count-failures,failures);Console.WriteLine(summary);
  File.WriteAllLines(Path.Combine(output,"test-output.txt"),results.Concat(new[]{summary}).ToArray(),new System.Text.UTF8Encoding(false));
  File.WriteAllText(Path.Combine(output,"results.json"),String.Format("{{\"passed\":{0},\"failed\":{1}}}",results.Count-failures,failures));
  return failures==0?0:1;
 }
 static int Demo(string output){
  Directory.CreateDirectory(output);string path=Path.Combine(output,"demo.json");if(File.Exists(path))throw new ArgumentException("Для демо укажите новый каталог.");
  var s=new Store(path);var b=Book(s,10,"М1");var m=MaterialsModule.Add(s,"Полировальная паста",5,1);
  JobsModule.Transition(s,b.Id,"М1","В работе");MaterialsModule.Consume(s,m.Id,b.Id,.2m,"DEMO-01");JobsModule.Transition(s,b.Id,"М1","Завершено");s.Save();
  var r=new Store(path);Console.WriteLine("АС БДС — демонстрация визита");
  Console.WriteLine("Клиент: "+r.Data.Bookings[0].Client);Console.WriteLine("Мастер: "+r.Data.Bookings[0].Master);
  Console.WriteLine("Статус: "+r.Data.Bookings[0].Status);
  Console.WriteLine("Материал: "+r.Data.Materials[0].Name);
  Console.WriteLine("Остаток: "+r.Data.Materials[0].Stock.ToString("0.00",CultureInfo.GetCultureInfo("ru-RU"))+" л");
  Console.WriteLine("После повторного открытия: 1 запись, 1 материал, 1 операция расхода");return 0;
 }
}
}

