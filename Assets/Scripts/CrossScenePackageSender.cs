using System;
using System.Collections;// se importa por que se me olvido quitarlo
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// el cartero interdimensional que envia paquetes entre escenas...
/// Advertencia abajo hay musica:
/// El cartero ya llego anunciando su cancion 
/// y grito con emocion: CORREO.
/// 
/// 
/// 
/// oh no referencia a Las Pistas De Blue. aka Blue's Clues
/// </summary>
public class CrossScenePackageSender : MonoBehaviour
{
    public static CrossScenePackageSender Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance == this)
            return;// no queremos Autodestruccion
		else Destroy(this.gameObject);
    }

    // Lista de paquetes
    public List<IPackage> Packages = new List<IPackage>();

    // Enviar paquete genérico
    public void SendPackage(IPackage package)
    {
        Packages.Add(package);
    }

    /// <summary>
    /// Enviar paquete normal
    /// </summary>
    /// <param name="sender">El objeto que manda el paquete</param>
    /// <param name="receiver">el receptor</param>
    /// <param name="contents">el contenido</param>
    /// <param name="tags">las etiquetas del paquete</param>
    [Obsolete]
    public void SendPackage(string sender, string receiver, object contents, string[] tags)
    {
        var pkg = new Package(sender, receiver, contents, tags);
        Packages.Add(pkg);
    }

    // Enviar paquete tipado
    public void SendTypedPackage<T>(string sender, string receiver, T contents, string[] tags)
    {
        var pkg = new TypedPackage<T>(sender, receiver, contents, tags);
        Packages.Add(pkg);
    }

    // Eliminar paquete
    public void DeleteMyPackage(IPackage package)
    {
        Packages.Remove(package);
    }

    // Revisar si hay paquetes para un GameObject (sin tipar)
    public bool IsThereAnyMailForHim(string Name, out List<IPackage> mail)
    {
        mail = Packages.FindAll(p => p.Reciever== Name);
        return mail.Count > 0;
    }

    // Revisar paquetes tipados para un GameObject
    public bool IsThereAnyTypedMailForHim<T>(string Name, out List<TypedPackage<T>> mail)
    {
        mail = new List<TypedPackage<T>>();
        foreach (var p in Packages)
        {
            if (p is TypedPackage<T> tp && tp.Reciever== Name)
            {
                mail.Add(tp);
            }
        }
        return mail.Count > 0;
    }
    // Revisar si hay paquetes para un GameObject (sin tipar)
    public bool IsThereAnyMailForMe(GameObject me, out List<IPackage> mail)
    {
        mail = Packages.FindAll(p => p.Reciever== me.name);
        return mail.Count > 0;
    }

    // Revisar paquetes tipados para un GameObject
    public bool IsThereAnyTypedMailForMe<T>(GameObject me, out List<TypedPackage<T>> mail)
    {
        mail = new List<TypedPackage<T>>();
        foreach (var p in Packages)
        {
            if (p is TypedPackage<T> tp && tp.Reciever== me.name)
            {
                mail.Add(tp);
            }
        }
        return mail.Count > 0;
    }
}

// INTERFAZ PARA TODOS LOS PAQUETES
public interface IPackage
{
    string Sender { get; }
    string Reciever{ get; }
    string[] Tags { get; }
}

// PAQUETE NORMAL (contenido como object)
[Serializable]
public class Package : IPackage
{
    public string Sender { get; set; }
    public string Reciever{ get; set; }
    public object Contents { get; set; }
    public string[] Tags { get; set; }

    public Package() { }

    public Package(string sender, string receiver, object contents, string[] tags)
    {
        Sender = sender;
        Reciever= receiver;
        Contents = contents;
        Tags = tags;
    }
}

// PAQUETE TIPADO (contenido fuertemente tipado)
[Serializable]
public class TypedPackage<T> : IPackage
{
    public string Sender { get; set; }
    public string Reciever{ get; set; }
    public T Contents { get; set; }
    public string[] Tags { get; set; }

    public TypedPackage() { }

    public TypedPackage(string sender, string receiver, T contents, string[] tags)
    {
        Sender = sender;
        Reciever= receiver;
        Contents = contents;
        Tags = tags;
    }
}