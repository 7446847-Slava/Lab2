using System;

public interface IWalkable { bool TryWalk(); }
public interface IRunnable { bool TryRun(); }
public interface IFlyable { bool TryFly(); }
public interface ICrawlable { bool TryCrawl(); }
public interface ITalkable { bool TryTalk(); }

public enum Habitat { Wild, Pet }

public abstract class Animal
{
    public string Name { get; protected set; }
    public bool IsAlive { get; private set; } = true;
    public bool IsHappy { get; private set; } = false;
    public int HoursSinceLastMeal { get; private set; } = 0;
    public Habitat CurrentHabitat { get; private set; }
    
    // ШАБЛОН OBSERVER (Спостерігач) - використання делегатів для подій
    public event Action<Animal, string> OnHungry;
    public event Action<Animal, string> OnDied;
    public event Action<Animal, string> OnActionInvoked;

    protected Animal(string name, Habitat habitat)
    {
        Name = name;
        CurrentHabitat = habitat;
    }

    public void PassTime(int hours)
    {
        if (!IsAlive) return;
        HoursSinceLastMeal += hours;
        
        if (HoursSinceLastMeal > 24) { Die(); return; }
        if (HoursSinceLastMeal >= 6) OnHungry?.Invoke(this, "Тварина зголодніла.");
        if (hours > 12) IsHappy = false; 
    }

    public void Feed() { if (!IsAlive) return; HoursSinceLastMeal = 0; LogAction("поїла."); }
    public void Clean() { if (!IsAlive) return; IsHappy = true; LogAction("знаходиться в чистоті і тепер щаслива!"); }
    private void Die() { IsAlive = false; OnDied?.Invoke(this, "помер від голоду..."); }
    protected void LogAction(string message) => OnActionInvoked?.Invoke(this, message);
    protected bool IsAliveCheck() => IsAlive;
    protected bool HasEnergyForComplexActions()
    {
        if (!IsAlive) return false;
        if (HoursSinceLastMeal > 8) { LogAction("занадто голодна. Їй треба поїсти!"); return false; }
        return true;
    }
}

public class Cat : Animal, IWalkable, IRunnable, ITalkable
{
    public Cat(string name, Habitat habitat) : base(name, habitat) { }
    public bool TryWalk() { if(!IsAliveCheck()) return false; LogAction("йде на лапах."); return true; }
    public bool TryRun() { if(!HasEnergyForComplexActions()) return false; LogAction("швидко біжить!"); return true; }
    public bool TryTalk() { if(!HasEnergyForComplexActions()) return false; LogAction("каже: Мяу!"); return true; }
}

public class Snake : Animal, ICrawlable
{
    public Snake(string name, Habitat habitat) : base(name, habitat) { }
    public bool TryCrawl() { if(!IsAliveCheck()) return false; LogAction("повзе по землі."); return true; }
}

public class Owner
{
    public string Name { get; private set; }
    public Animal Pet { get; private set; }
    public Owner(string name) => Name = name;

    public void AdoptPet(Animal animal)
    {
        Pet = animal;
        // Підписка на подію
        Pet.OnHungry += HandlePetHungry;
    }

    private void HandlePetHungry(Animal animal, string message)
    {
        Console.WriteLine($"\n[Подія] Хазяїн {Name} помітив: {message} Годуємо {animal.Name}.");
        animal.Feed();
    }
    public void CleanPetArea() { if (Pet != null) { Console.WriteLine($"\n[Подія] Хазяїн {Name} прибирає."); Pet.Clean(); } }
}

class Program
{
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("=== Симуляція (Шаблон Observer) ===\n");

        Cat myCat = new Cat("Барсік", Habitat.Pet);
        Owner owner = new Owner("Олександр");
        owner.AdoptPet(myCat);

        myCat.OnActionInvoked += (animal, msg) => Console.WriteLine($"[{animal.Name}]: {msg}");
        myCat.TryWalk();
        myCat.PassTime(9); 
        myCat.TryRun();
    }
}