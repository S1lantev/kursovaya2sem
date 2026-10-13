using System;
using System.Linq;
namespace DetailingCourse {
public class MaterialsModule:IModule{
 public int Order {get{return 3;}}public string Name {get{return "Материалы и химия";}}
 public static Material Add(Store s,string name,decimal stock,decimal threshold) {
  name=Store.Text(name,"название");if(stock<0||threshold<0)throw new ArgumentException("Остаток и порог неотрицательны.");
  if(s.Data.Materials.Any(m=>String.Equals(m.Name,name,StringComparison.OrdinalIgnoreCase)))throw new ArgumentException("Материал уже есть.");
  var m=new Material{Id=s.Data.Materials.Count==0?1:s.Data.Materials.Max(x=>x.Id)+1,Name=name,Stock=stock,Threshold=threshold,Reorder=stock<threshold};
  s.Data.Materials.Add(m);return m;
 }
 public static void Consume(Store s,int id,int booking,decimal qty,string operation) {
  operation=Store.Text(operation,"операция"); var m=s.Material(id);var b=s.Booking(booking);
  if(b.Status=="Отменено")throw new ArgumentException("Нельзя списать расход на отменённую запись.");
  if(qty<=0)throw new ArgumentException("Расход должен быть больше нуля.");
  if(s.Data.Usages.Any(u=>u.Operation==operation))throw new ArgumentException("Эта операция уже учтена.");
  if(qty>m.Stock)throw new ArgumentException("Недостаточный остаток.");
  m.Stock-=qty;m.Reorder=m.Stock<m.Threshold;
  s.Data.Usages.Add(new Usage{Operation=operation,BookingId=booking,MaterialId=id,Quantity=qty});
 }
 public void Run(Store s){
  int c=Menu.Choose(Name,new[]{"Добавить материал","Остатки и потребность в пополнении","Списать расход","Назад"});if(c<0||c==3)return;
  if(c==0){Add(s,Menu.Input("Название"),Menu.Amount("Остаток, л"),Menu.Amount("Минимальный остаток, л"));s.Save();Console.WriteLine("Материал сохранён.");}
  if(c==1)foreach(var m in s.Data.Materials)Console.WriteLine("{0}: {1} — {2} л {3}",m.Id,m.Name,m.Stock,m.Reorder?"(нужно пополнить)":"");
  if(c==2){Consume(s,Menu.Number("ID материала"),Menu.Number("ID записи"),Menu.Amount("Расход, л"),Menu.Input("Номер операции"));s.Save();Console.WriteLine("Расход учтён.");}Menu.Pause();
 }
}
}
