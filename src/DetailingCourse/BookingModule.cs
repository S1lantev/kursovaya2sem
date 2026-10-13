using System;
using System.Linq;
namespace DetailingCourse {
public class BookingModule:IModule {
 public int Order {get{return 1;}} public string Name {get{return "Запись клиентов";}}
 public static Booking Add(Store s,string client,string car,string service,string master,DateTime start,int minutes) {
  client=Store.Text(client,"клиент");car=Store.Text(car,"автомобиль");service=Store.Text(service,"услуга");master=Store.Text(master,"мастер");
  if(minutes<=0||minutes>480) throw new ArgumentException("Длительность: от 1 до 480 минут.");
  DateTime end=start.AddMinutes(minutes);
  if(start.TimeOfDay<TimeSpan.FromHours(10)||end.Date!=start.Date||end.TimeOfDay>TimeSpan.FromHours(18)) throw new ArgumentException("Рабочее время: 10:00–18:00.");
  if(s.Data.Bookings.Any(b=>b.Status!="Отменено"&&String.Equals(b.Master,master,StringComparison.OrdinalIgnoreCase)&&
   start<b.Start.AddMinutes(b.Minutes)&&end>b.Start)) throw new ArgumentException("Мастер занят в выбранное время.");
  var result=new Booking{Id=s.Data.Bookings.Count==0?1:s.Data.Bookings.Max(b=>b.Id)+1,Client=client,Car=car,Service=service,Master=master,Start=start,Minutes=minutes};
  s.Data.Bookings.Add(result); return result;
 }
 public static void Cancel(Store s,int id) {var b=s.Booking(id);if(b.Status!="Запланировано")throw new ArgumentException("Отменяется только запланированная запись."); b.Status="Отменено";}
 public void Run(Store s) {
  int c=Menu.Choose(Name,new[]{"Создать запись","Показать календарь","Отменить запись","Назад"});if(c<0||c==3)return;
  if(c==0){Add(s,Menu.Input("Клиент"),Menu.Input("Автомобиль"),Menu.Input("Услуга"),Menu.Input("Мастер"),Menu.Date(),Menu.Number("Длительность, мин"));s.Save();Console.WriteLine("Запись сохранена.");}
  if(c==1)foreach(var b in s.Data.Bookings.OrderBy(x=>x.Start))Console.WriteLine("{0}: {1:dd.MM.yyyy HH:mm} {2}, {3}, {4} — {5}",b.Id,b.Start,b.Client,b.Service,b.Master,b.Status);
  if(c==2){Cancel(s,Menu.Number("ID записи"));s.Save();Console.WriteLine("Запись отменена.");}Menu.Pause();
 }
}
}
