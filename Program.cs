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
    
    public bool HasPaws { get; protected set; }
    public bool HasWings { get; protected set; }

    public event Action<Animal> OnHungry;
    public event Action<Animal> OnDied;
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
        
        if (HoursSinceLastMeal > 24)
        {
            Die();
            return;
        }

        if (HoursSinceLastMeal >= 6) 
        {
            OnHungry?.Invoke(this);
        }
        
        if (hours > 12) IsHappy = false; 
    }

    public void Feed()
    {
        if (!IsAlive) return;
        HoursSinceLastMeal = 0;
        LogAction("поїла.");
    }

    public void Clean()
    {
        if (!IsAlive) return;
        IsHappy = true;
        LogAction("знаходиться в чистоті і тепер щаслива!");
    }

    private void Die()
    {
        IsAlive = false;
        OnDied?.Invoke(this);
    }

    protected bool HasEnergyForComplexActions()
    {
        if (!IsAlive) return false;
        if (HoursSinceLastMeal > 8)
        {
            LogAction("занадто голодна для цієї дії. Їй треба поїсти!");
            return false;
        }
        return true;
    }
    
    protected bool IsAliveCheck() => IsAlive;

    protected void LogAction(string message)
    {
        OnActionInvoked?.Invoke(this, message);
    }
}

public class Cat : Animal, IWalkable, IRunnable, ITalkable
{
    public Cat(string name, Habitat habitat) : base(name, habitat) { HasPaws = true; }
    public bool TryWalk() { if(!IsAliveCheck()) return false; LogAction("йде на лапах."); return true; }
    public bool TryRun() { if(!HasEnergyForComplexActions()) return false; LogAction("швидко біжить!"); return true; }
    public bool TryTalk() { if(!HasEnergyForComplexActions()) return false; LogAction("каже: Мяу!"); return true; }
}

public class Parrot : Animal, IWalkable, IFlyable, ITalkable
{
    public Parrot(string name, Habitat habitat) : base(name, habitat) { HasPaws = true; HasWings = true; }
    public bool TryWalk() { if(!IsAliveCheck()) return false; LogAction("смішно крокує."); return true; }
    public bool TryFly() { if(!HasEnergyForComplexActions()) return false; LogAction("літає по кімнаті!"); return true; }
    public bool TryTalk() { if(!HasEnergyForComplexActions()) return false; LogAction("каже: Піастри!"); return true; }
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

    public Owner(string name)
    {
        Name = name;
    }

    public void AdoptPet(Animal animal)
    {
        if (Pet != null) throw new InvalidOperationException("Хазяїн може мати лише одну тварину!");
        
        Pet = animal;
        Pet.OnHungry += HandlePetHungry;
    }

    private void HandlePetHungry(Animal animal)
    {
        Console.WriteLine($"\n[Подія] Хазяїн {Name} помітив, що {animal.Name} голодна, і нагодував її.");
        animal.Feed();
    }

    public void CleanPetArea()
    {
        if (Pet != null)
        {
            Console.WriteLine($"\n[Подія] Хазяїн {Name} прибирає за {Pet.Name}.");
            Pet.Clean();
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("=== Симуляція Життя Тварин ===\n");

        Cat myCat = new Cat("Барсік", Habitat.Pet);
        Owner owner = new Owner("Олександр");
        owner.AdoptPet(myCat);

        myCat.OnActionInvoked += (animal, message) => Console.WriteLine($"[{animal.Name}]: {message}");
        myCat.OnDied += (animal) => Console.WriteLine($"\n[ТРАГЕДІЯ]: {animal.Name} помер від голоду...");

        Console.WriteLine("--- Ранок ---");
        myCat.TryWalk();
        myCat.TryTalk();

        Console.WriteLine("\n--- Минуло 9 годин ---");
        myCat.PassTime(9); 
        
        myCat.TryRun();

        owner.CleanPetArea();
        Console.WriteLine($"Чи щасливий {myCat.Name}? {(myCat.IsHappy ? "Так" : "Ні")}");

        Console.WriteLine("\n=== Демонстрація дикої природи ===");
        Snake wildSnake = new Snake("Снейк", Habitat.Wild);
        wildSnake.OnActionInvoked += (animal, message) => Console.WriteLine($"[{animal.Name}]: {message}");
        wildSnake.OnDied += (animal) => Console.WriteLine($"\n[ТРАГЕДІЯ]: {animal.Name} вмерла...");
        
        wildSnake.TryCrawl();
        
        Console.WriteLine("\n--- Минуло 10 годин ---");
        wildSnake.PassTime(10); 
        
        wildSnake.TryCrawl(); 
        
        Console.WriteLine("\n--- Минуло ще 15 годин ---");
        wildSnake.PassTime(15); 
        
        wildSnake.TryCrawl(); 
        
        Console.ReadLine();
    }
}