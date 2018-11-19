using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Collections;
using FB;
using InSAT.Library.Interop;
using System.IO;
using System.Globalization;

namespace FB_Stages
{
	[Serializable,
	 ComVisible(true),
	 Guid("BD3BA009-E0CE-4C8A-932F-4EC447D7D01A"),
	 CatID(CatIDs.CATID_OTHER),
	 DisplayName("Стадийное управление"),
	 ControllerCode(54),
	 FBOptions(FBOptions.UseScanByTime)]
	
	public class Stages_control : StaticFBBase
	{
		const int Pin = 1;
		const int Pin2 = 2;
		const int Pin3 = 3;
		const int Pin4 = 4;
		const int Pin5 = 5;
		const int Pin6 = 6;
		const int Pin7 = 7;
		const int Pin8 = 8;

		const int Pout = 9;
		const int Pout1 = 10;
		const int Pout2 = 11;
		const int Pout3 = 12;
		const int Pout4 = 13;

		static double[,] stages = new double[100, 3];
		double[][,] rec = new double[10][,];//массив рецептов или массив массивов стадий
		////////////////////////////////////////////////////////////ВХОДА
		static int in_stage_number;//номер стадии на входе фб
		double in_volume;// заданный объем на входе фб
		double in_start_conc;//заданная начальная концентрация на входе фб
		double in_final_conc;//заданная конечная концентрация на входе фб
		static int in_recipe_number;// номер рецепта
		bool acpt_write;// разрешение на запись
		bool recipe_write;// записать рецепт
		bool recipe_read;// считать рецепт
						 ////////////////////////////////////////////////////////////ВЫХОДА
		int out_stage_number;//номер стадии на выходе фб
		double out_volume;// заданный объем на выходе фб
		double out_start_conc;//заданная начальная концентрация на выходе фб
		double out_final_conc;//заданная конечная концентрация на выходе фб
		int read_recipe;// считанный рецепт

        static string path; //путь для папки
		//public static double[,] mas = new double[10, 3];
		static int rows = stages.GetUpperBound(0) + 1; //переменная для пересчета строк
		static int columns = stages.Length / rows; //переменная для пересчета столбцов
		//double[,] masout = new double[rows, columns];
		void Write()//функциия записи входных параметров
		{
			if (acpt_write)
			{
				stages[in_stage_number, 0] = in_volume;
				stages[in_stage_number, 1] = in_start_conc;
				stages[in_stage_number, 2] = in_final_conc;
			}
		}
		void Out()//функция вывода выходных параметров фб
		{
			read_recipe = in_recipe_number;
			out_stage_number = in_stage_number;
			out_volume = stages[in_stage_number, 0];
			out_start_conc= stages[in_stage_number, 1];
			out_final_conc = stages[in_stage_number, 2];
		}
		/*void Recipe()//функция обработки рецептов
		{
			if (recipe_write)//запись в массив рецептов
			{	for (int i=0; i<stages.GetLength(0); i++)
				{
					for (int j = 0; j < stages.GetLength(1); j++)
					{
						rec[in_recipe_number][i, j] = stages[i, j];
					}
				}
			}
			if (recipe_read)//чтение из массива рецептов
			{
				for (int i = 0; i < stages.GetLength(0); i++)
				{
					for (int j = 0; j < stages.GetLength(1); j++)
					{
						stages[i, j]= rec[in_recipe_number][i, j];
					}
				}
			}
		}*/
		void SaveToFile()
		{	
			if (recipe_write)
			{
				string razdelenie = ";";  //переманная для разделения
				string perenos = "\r\n"; //переменная для переноса
				string specifier;  //переменная для преобразования значений в биты      
				using (FileStream fs = File.Create(path)) //открытие пути файла
				{
					for (int i = 0; i < rows; i++) //пересчет строк
					{
						for (int j = 0; j < columns; j++) //пересчет столбцов в каждой строке
						{
							specifier = stages[i, j].ToString("G", CultureInfo.CreateSpecificCulture("fr-FR")); //преобразование каждого значения в строчный тип
							byte[] array = System.Text.Encoding.Default.GetBytes(specifier); //преобразование строчного значения в битовое
							if (j != 0) // условие для разделения значений
							{
								byte[] brzdl = System.Text.Encoding.Default.GetBytes(razdelenie); //преобразование разделителям в байты
								fs.Write(brzdl, 0, brzdl.Length); //запись разделителя
							}
							fs.Write(array, 0, array.Length); //запись битового значения
						}
						byte[] bprns = System.Text.Encoding.Default.GetBytes(perenos); //преобразование переноса в биты
						fs.Write(bprns, 0, bprns.Length); //запись переноса
					}
				}
			}
				
		}
		void DownloadFromFile()
		{
			if (recipe_read)
			{
				using (FileStream fs2 = File.OpenRead(path))         //начало описания алгоритма возврата значений в матрицу
				{
					byte[] arrayout = new byte[fs2.Length];
					fs2.Read(arrayout, 0, arrayout.Length);  //битовые значения из файла
					string textFromFile = System.Text.Encoding.Default.GetString(arrayout); //преобразования битовых значений в string
					textFromFile = textFromFile.Replace("\r\n", ";"); //корректировка переносов & разделений
					string[] words = textFromFile.Split(new char[] { ';' }); //разбиение значений в массив
					int q = 0; //ввод счетчика считываемого массива
					for (int y = 0; y < rows; y++) //пересчет строк
					{
						for (int x = 0; x < columns; x++) //пересчет столбцов
						{
							double t = Convert.ToDouble(words[q]); //преобразование переменной в double
							stages[y, x] = t; // присвоение массиву
											  //Console.WriteLine(masout[x, y]);
							q += 1;
						}
					}

				}
			}
			
		}
		protected override void UpdateData()
        {
			Out();
			in_stage_number = GetPinInt(Pin);
			in_volume = GetPinDouble(Pin2);
			in_start_conc = GetPinDouble(Pin3);
			in_final_conc = GetPinDouble(Pin4);
			in_recipe_number = GetPinInt(Pin5);
			acpt_write = GetPinBool(Pin6);
			recipe_write = GetPinBool(Pin7);
			recipe_read = GetPinBool(Pin8);

			SetPinValue(Pout,out_volume);
			SetPinValue(Pout1, out_start_conc);
			SetPinValue(Pout2, out_final_conc);
			SetPinValue(Pout3, out_stage_number);
			SetPinValue(Pout4, read_recipe);
			Write();
			path = string.Format(@"c:\note\note{0}.txt", Convert.ToString(in_recipe_number));
			SaveToFile();
			DownloadFromFile();
		}
		
	}
}