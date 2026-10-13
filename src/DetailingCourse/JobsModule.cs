using System;
using System.Linq;
namespace DetailingCourse {
public class JobsModule:IModule {
 public int Order {get{return 2;}}public string Name {get{return "Задания мастеров";}}
 public static void Transition(Store s,int id,string actor,string target) {
  var b=s.Booking(id); if(!String.Equals(b.Master,Store.Text(actor,"мастер"),StringComparison.OrdinalIgnoreCase))throw new ArgumentException("Изменять можно только свои задания.");
  bool allowed=b.Status=="Запланировано"&&target=="В работе"||b.Status=="В работе"&&target=="Завершено";
  if(!allowed)throw new ArgumentException("Недопустимый переход статуса.");b.Status=target;
 }
 public void Run(Store s){
  string master=Menu.Input("Мастер");int c=Menu.Choose(Name,new[]{"Мои задания","Начать работу","Завершить работу","Назад"});if(c<0||c==3)return;
  if(c==0)foreach(var b in s.Data.Bookings.Where(x=>String.Equals(x.Master,master,StringComparison.OrdinalIgnoreCase)).OrderBy(x=>x.Start))
   Console.WriteLine("{0}: {1:dd.MM.yyyy HH:mm}, {2}, {3} — {4}",b.Id,b.Start,b.Client,b.Service,b.Status);
  else{Transition(s,Menu.Number("ID задания"),master,c==1?"В работе":"Завершено");s.Save();Console.WriteLine("Статус сохранён.");}Menu.Pause();
 }
}
}
