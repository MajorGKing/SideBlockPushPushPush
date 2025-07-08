using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;

public class ObjectManager
{
    public ObjectManager()
    {
    }

    public void Clear()
    {
        RemoveHero();
        RemoveAllMonsters();
        RemoveAllSkillEffects();
        RemoveAllBuddies();
    }

    #region Roots
    public Transform GetRootTransform(string name)
    {
        GameObject root = GameObject.Find(name);
        if (root == null)
            root = new GameObject { name = name };

        return root.transform;
    }

    public Transform HeroRoot { get { return GetRootTransform("@Heroes"); } }
    public Transform MonsterRoot { get { return GetRootTransform("@Monsters"); } }
    public Transform ProjectileRoot { get { return GetRootTransform("@Projectiles"); } }
    public Transform EnvRoot { get { return GetRootTransform("@Envs"); } }
    public Transform EffectRoot { get { return GetRootTransform("@Effects"); } }
    public Transform NpcRoot { get { return GetRootTransform("@Npc"); } }
    public Transform ItemHolderRoot { get { return GetRootTransform("@ItemHolders"); } }
    #endregion

    public HeroController Hero { get; private set; }
    public HashSet<MonsterController> Monsters { get; private set; } = new HashSet<MonsterController>();
    public List<MonsterController> LivingMonsterList
    {
        get { return Monsters.Where(monster => monster.IsAlive).ToList(); }
    }

    public HashSet<BuddyController> Buddies { get; private set; } = new HashSet<BuddyController>();

    public List<SkillEffect> SkillEffects { get; private set; } = new List<SkillEffect>();

    #region Spawn

    private const string HERO_PREFAB_NAME = "Hero";
    private const string BUDDY_PREFAB_NAME = "Buddy";
    private const string MONSTER_PREFAB_NAME = "Monster";
    private const string NORMAL_MONSTER_PREFAB_NAME = "NormalMonster";
    private const string BOSS_MONSTER_PREFAB_NAME = "BossMonster";

    

    public GameObject SpawnGameObject(Vector3 position, string prefabName)
    {
        GameObject go = Managers.Resource.Instantiate(prefabName, pooling: true);
        go.transform.position = position;
        
        return go;
    }

    public T SpawnCreatureObject<T>(Transform parent, int templateID) where T : CreatureController
    {
        T creatureObject = null;

        if(typeof(T) == typeof(HeroController))
        {
            creatureObject = SpawnHero(parent, templateID) as T;
        }
        else if (typeof(T) == typeof(BuddyController))
        {
            creatureObject = SpawnBuddy(parent, templateID) as T;
        }
        else if (typeof(T) == typeof(MonsterController))
        {
            creatureObject = SpawnMonster(parent, templateID) as T;
        }

        if(creatureObject != null)
        {
            return creatureObject;
        }

        return null;
    }

    private HeroController SpawnHero(Transform parent, int templateID)
    {
        HeroController hero = Managers.Resource.Instantiate(HERO_PREFAB_NAME, parent).GetComponent<HeroController>();
        if(hero != null)
        {
            Hero = hero;
            hero.SetInfo(templateID);
            return hero;
        }
        return null;
    }

    private BuddyController SpawnBuddy(Transform parent, int templateID)
    {
        BuddyController buddy = Managers.Resource.Instantiate(BUDDY_PREFAB_NAME, parent).GetComponent<BuddyController>();
        if (buddy != null)
        {
            buddy.SetInfo(templateID);
            Buddies.Add(buddy);
            return buddy;
        }

        return null;
    }

    // TODO Set Level
    private MonsterController SpawnMonster(Transform parent, int templateID)
    {
        MonsterController normalMonster = Managers.Resource.Instantiate(MONSTER_PREFAB_NAME, parent).GetComponent<MonsterController>();
        if (normalMonster != null)
        {
            normalMonster.SetInfo(templateID);
            Monsters.Add(normalMonster);
            return normalMonster;
        }

        return null;
    }

    public SkillEffect SpawnSkillEffect(Vector3 position, string prefabName, float lifeTime)
    {
        SkillEffect skillEffect = Managers.Resource.Instantiate(prefabName).GetOrAddComponent<SkillEffect>();
        if (skillEffect != null)
        {
            skillEffect.SetInfo(lifeTime);
            skillEffect.transform.position = position;
            SkillEffects.Add(skillEffect);
            return skillEffect;
        }

        return null;
    }

    #endregion

    public void DespawnGameObject<T>(T obj) where T : BaseController
    {

    }

    #region Remove

    public void RemoveHero()
    {
        if (Hero != null)
        {
            Managers.Resource.Destroy(Hero.gameObject);
            Hero = null;
        }
    }

    public void RemoveBuddy(BuddyController buddy)
    {
        if(Buddies.Contains(buddy))
        {
            Managers.Resource.Destroy(buddy.gameObject);
            Buddies.Remove(buddy);
        }
    }

    public void RemoveAllBuddies()
    {
        foreach (BuddyController buddy in Buddies)
        {
            Managers.Resource.Destroy(buddy.gameObject);
        }

        Buddies.Clear();
    }

    public void RemoveMonster(MonsterController monster)
    {
        if (Monsters.Contains(monster))
        {
            Managers.Resource.Destroy(monster.gameObject);
            Monsters.Remove(monster);
        }
    }

    public void RemoveAllMonsters()
    {
        foreach (CreatureController monster in Monsters)
        {
            Managers.Resource.Destroy(monster.gameObject);
        }

        Monsters.Clear();
    }

    public void RemoveSkillEffect(SkillEffect skillEffect)
    {
        if (SkillEffects.Contains(skillEffect))
        {
            Managers.Resource.Destroy(skillEffect.gameObject);
            SkillEffects.Remove(skillEffect);
        }
    }

    public void RemoveAllSkillEffects()
    {
        foreach (SkillEffect skillEffect in SkillEffects)
        {
            Managers.Resource.Destroy(skillEffect.gameObject);
        }

        SkillEffects.Clear();
    }
    #endregion

}