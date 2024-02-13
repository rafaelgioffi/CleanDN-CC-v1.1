using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CleanDN_CC
{
    public class Program
    {
        static void Main(string[] args)
        {
            string DNIgn = ConfigurationManager.AppSettings["IgnoredDNs"];
            string LogFile = ConfigurationManager.AppSettings["LogFile"];
            string Folder = ConfigurationManager.AppSettings["FolderToProcessFiles"];
            string FolderToResult = ConfigurationManager.AppSettings["FolderToResultedFiles"];
            string FileToProcess = ConfigurationManager.AppSettings["FileToProcess"];
            double MaxValue = double.Parse(ConfigurationSettings.AppSettings["MaxValue"]);
            string[] filesInFolder;
            int fileQuantity = 0;

            Log("\n################### Inicio do processamento ###################", true);
            Console.WriteLine("################### Inicio do processamento ###################");
            try
            {
                filesInFolder = Directory.GetFiles(Folder, FileToProcess + "*");  //procura os LHDIFs que tiverem na pasta
                fileQuantity = filesInFolder.Length;  //quantidade de arquivos encontrados
            }
            catch (Exception ex)
            {
                Log($"\nNenhum arquivo {FileToProcess} encontrado! Ignorando o processamento...\n\n", false);
                Console.WriteLine($"\nNenhum arquivo {FileToProcess} encontrado! Ignorando o processamento...\n\n");
                return;
            }
            
            string FileValidName = "";    //nome do novo arquivo COM o(s) DN(s) listados e com valores válidos
            
            List<string> FileValid = new List<string>();

            if (fileQuantity > 0)
            {
                Log($"Iniciando o processamento de {fileQuantity} arquivo(s).",false);
                Console.WriteLine($"Iniciando o processamento de {fileQuantity} arquivo(s).");
                int counter = 1;
                foreach (var file in filesInFolder) //processa arquivo por arquivo encontrado na pasta...
                {
                    FileValid.Clear();
                    Log($"Processando o arquivo {counter}/{fileQuantity}...", false);
                    Console.WriteLine($"Processando o arquivo {counter}/{fileQuantity}...");
                    try
                    {
                        string[] allLines = File.ReadAllLines(file);
                        string[] procDate = allLines[0].Split('#');
                        string procDateFiltered = DateTime.Parse(procDate[1]).ToString("yyyyMMdd");
                        string timeFile = File.GetCreationTime(file).ToString("HHmmss");

                        foreach (var l in allLines) //validação de cada linha...
                        {
                            string[] DnActual = l.Split('#');
                            double DocValue = 0;
                            double.TryParse(DnActual[5], out DocValue);

                            if (DnActual[0] == DNIgn)
                            {
                                if (DocValue <= MaxValue)
                                FileValid.Add(l);
                            }                            
                        }

                        if (FileValid.Count > 0)  //só executa alguma ação se encontrar algum DN na validação acima...
                        {
                            string[] actual = file.Split('\\');
                            string actualName = actual.Last();
                            actual = actualName.Split('.');
                            actualName = $"{actual[0]}.{actual[1]}";                            

                            FileValidName = $"{actualName}.TXT.{procDateFiltered}_{timeFile}";   //nome do arquivo válido...

                            if (!Directory.Exists(FolderToResult)) { Directory.CreateDirectory(FolderToResult); }    //cria a pasta de resultado caso não exista...

                            if (File.Exists(FolderToResult + @"\" + FileValidName))
                            {
                                int time = int.Parse(timeFile) + 3;
                                FileValidName = $"{actualName}.TXT.{procDateFiltered}_{time}";
                            }

                            using (StreamWriter sw = new StreamWriter(FolderToResult + FileValidName)) //cria o arquivo somente dos banidos...
                            {
                                foreach (string newLines in FileValid)
                                {
                                    sw.WriteLine(newLines);
                                }
                            }
                            Log($"Gerado o arquivo com o(s) DN(s) {DNIgn} em {FolderToResult}{FileValidName}...", false);
                            Console.WriteLine($"Gerado o arquivo com o(s) DN(s) {DNIgn} em {FolderToResult}{FileValidName}...");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log($"Erro ao processar o arquivo {file}. {ex.Message}", false);
                        Console.WriteLine($"Erro ao processar o arquivo {file}.\n{ex.Message}");
                    }
                    counter++;
                }
            }
            else
            {
                Log($"Nenhum arquivo {FileToProcess} para processar em {Folder}", false);
                Console.WriteLine($"Nenhum arquivo {FileToProcess} para processar em {Folder}");
            }
            
            Log("########## Fim do processamento ##########\n", true);
            Console.WriteLine("########## Fim do processamento ##########\n");

            void Log(string message, bool special)
            {
                using (StreamWriter swLog = new StreamWriter(LogFile, true))
                {
                    if (special)
                    {
                        swLog.WriteLine(message);
                    }
                    else
                    {
                    swLog.WriteLine($"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")} => {message}");
                    }
                }
            }

        }
    }
}
