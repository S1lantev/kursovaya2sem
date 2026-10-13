using System;
using System.Globalization;
namespace DetailingCourse {
public static class Menu {
 public static int Move(int selected,int delta,int count) { return (selected+delta+count)%count; }
 public static int Choose(string title,string[] items) {
  int selected=0; while(true) {
   Console.Clear(); Console.WriteLine(title); Console.WriteLine("Меню раскрыто: стрелки, Enter — выбор, Escape — назад.");
   for(int i=0;i<items.Length;i++) Console.WriteLine((i==selected?" > ":"   ")+items[i]);
   var key=Console.ReadKey(true).Key;
   if(key==ConsoleKey.UpArrow) selected=Move(selected,-1,items.Length);
   if(key==ConsoleKey.DownArrow) selected=Move(selected,1,items.Length);
   if(key==ConsoleKey.Enter) return selected; if(key==ConsoleKey.Escape) return -1;
  }
 }
 public static string Input(string name) { Console.Write(name+": "); return Store.Text(Console.ReadLine(),name); }
 public static int Number(string name) { int n; if(!Int32.TryParse(Input(name),out n)) throw new ArgumentException("Нужно целое число."); return n; }
 public static decimal Amount(string name) { decimal n; if(!Decimal.TryParse(Input(name),NumberStyles.Number,CultureInfo.GetCultureInfo("ru-RU"),out n)) throw new ArgumentException("Нужно число, например 0,20."); return n; }
 public static DateTime Date() { DateTime d; if(!DateTime.TryParseExact(Input("Дата и время (дд.ММ.гггг ЧЧ:мм)"),"dd.MM.yyyy HH:mm",CultureInfo.InvariantCulture,DateTimeStyles.None,out d)) throw new ArgumentException("Неверная дата."); return d; }
 public static void Pause() { Console.WriteLine("Нажмите любую клавишу."); Console.ReadKey(true); }
}
}
