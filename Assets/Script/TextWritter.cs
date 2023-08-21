using System.IO;
using UnityEngine;

public class TextWriter : MonoBehaviour
{
    public static string fileName = "test.compo.json"; // Nom du fichier à créer/écrire
    public static string path = "C:\\Users\\Tristan\\Documents\\GitProjects\\deathtower\\res\\editor\\compo";//"C:\\Users\\tristan\\Documents\\deathtower\\res\\editor\\compo"; //

    public static void Write(string textToWrite)
    {
        // Chemin complet du fichier
        string filePath = Path.Combine(path, fileName); // Application.persistentDataPath

        // Écriture du texte dans le fichier
        File.WriteAllText(filePath, textToWrite);

        Debug.Log("Le texte a été écrit dans le fichier : " + filePath);
    }
}