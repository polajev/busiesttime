/*
 * A program that takes data about the drivers' breaks and gives out the time with the most break taking drivers. 
 * Input data format: <start time><end time>. Example: 10:3011:35
 * The user can either input the times in the terminal or give a path to a textfile as a command line argument. 
 * Example of providing file: .\busiesttime.exe filename "C:\Users\me\Documents\testtime.txt"
 */
using System;
using System.Linq;

//Convert the string array of times (<start time><end time>) to a TimeSpan object of start and end times, 2-D array. Get earliest and latest time. 
class TextToTime
{
	//The actual earliest and latest time will be found afterwards through comparison. 
	public TimeSpan earliest = new TimeSpan(23, 59, 00);
	public TimeSpan latest = new TimeSpan(00, 00, 00);
	public TimeSpan[,] startEnd;
	
	public TextToTime(string[] textLines)
	{
		string[] lines = textLines;
		//Convert to TimeSpan values, start and end. Column 0 - start time, column 1 - end time.
			startEnd = new TimeSpan[lines.Length,2];
			for (int i = 0; i < lines.Length; i++)
			{
				//Get the right substrings from the raw data. Convert them to int. First two characters are the hour, etc.
				startEnd[i,0] = new TimeSpan(int.Parse(lines[i].Substring(0,2)), int.Parse(lines[i].Substring(3,2)), 00);
				startEnd[i,1] = new TimeSpan(int.Parse(lines[i].Substring(5,2)), int.Parse(lines[i].Substring(8,2)), 00);
				
				//Find the min start time and max end time values along the way. 
				if (startEnd[i,0] < earliest)
				{
					earliest = startEnd[i,0];
				}
				if (startEnd[i,1] > latest)
				{
					latest = startEnd[i,1];
				}
			}
	}
}

//Create arrays to measure busyness. Length: distance between min and max time, in minutes, to catch everything.
class TimeAndDrivers
{
	//This will be the number of drivers on breaks at a given time.
	public int[] busyTimes;
	//This will record the exact time when x number of drivers are on a break.
	public TimeSpan[] theTime;
	
	public TimeAndDrivers(int howLong, TextToTime timeData)
	{
			//Create arrays to measure busyness. Length: distance between min and max time, in minutes, to catch everything. 
			busyTimes = new int[howLong];
			theTime = new TimeSpan[howLong];
			
			//Fill the theTime array with the exact times. 
			TimeSpan oneMinute = new TimeSpan(00, 01, 00);
			//Start with the earliest break time we have and fill in the rest. 
			TimeSpan current = timeData.earliest;
			for (int i = 0; i < howLong; i++)
			{
				theTime[i] = current;
				current = current + oneMinute;
			}
			
			//Sum up all the people on a break, in their exact minutes. 
			int startIndex;
			int endIndex;
			for (int i = 0 ; i < timeData.startEnd.GetLength(0); i++)
			{
				//Find where in the array does this person's break start and end.
				startIndex = Array.FindIndex(theTime, x => x.Equals(timeData.startEnd[i,0]));
				endIndex = Array.FindIndex(theTime, x => x.Equals(timeData.startEnd[i,1]));
				
				//Add this person's break, adding +1 to his area in the busyTimes array.
				for (int j = startIndex; j < (endIndex + 1); j++)
				{
					busyTimes[j]++;
				}
			}
	}
}

//Find the final answers to this assignment. Takes class TimeAndDrivers's attribute busyTimes as input. 
class GetAnswer
{
	//Largest number of drivers on break at a time.
	public int maxValue;
	//Beginning and end of the "busiest period", with the max number of drivers on break.
	public int maxIndex;
	public int lastIndex;
	
	//noOfDrivers is the array with the number of drivers on break for different times. 
	public GetAnswer(int[] noOfDrivers)
	{
		maxValue = noOfDrivers.Max();
		maxIndex = noOfDrivers.ToList().IndexOf(maxValue);
		
		//Start searching for where the busy period ends. 
			lastIndex = maxIndex;
			while (lastIndex < noOfDrivers.Length)
			{
				//Once the current value is less than max (busiest time), we found the end.
				if (noOfDrivers[lastIndex] < maxValue)
				{
					break;
				} else 
				{
					lastIndex++;
				}
				
			}
	}
}

namespace busiesttime
{
	class Program
	{
		public static void Main(string[] args)
		{
			
			//If the user used the switch "filename" and provided path, use the path. 
			if (args.Length == 2 && args[0] == "filename")
			{
				// Read the textfile with time <start time><end time>
				string[] lines = System.IO.File.ReadAllLines(args[1]);
						
				//Convert the time data to a more workable format, using a custom class. timeData.startEnd will be a 2-d array of TimeSpan.
				TextToTime timeData = new TextToTime(lines);
									
				//Distance between min and max values, in minutes
				int minToMax = Convert.ToInt32((timeData.latest - timeData.earliest).TotalMinutes) + 1;
			
				//Create arrays that contain times (theTime) and number of busy drivers (busyTimes), using a custom class. 
				TimeAndDrivers summary = new TimeAndDrivers(minToMax, timeData);
			
				//Find the answer from the array of drivers on break. 
				GetAnswer theAnswer = new GetAnswer(summary.busyTimes);			
			
				//Print the answer. Start and end of busiest period. 
				Console.WriteLine("The busiest period is " + summary.theTime[theAnswer.maxIndex] + "-" + summary.theTime[theAnswer.lastIndex-1] + ", with a total of " + theAnswer.maxValue + " drivers taking a break then.");
				
				//Allow user to add more times, in addition to the text file.

				string userInput;
			
				while (true)
				{
					Console.Write("You can add more times, format <start time><end time>. Or press Enter : ");
					userInput = Console.ReadLine();
					//If user writes nothing, just pushes Enter, then exit.
					if (userInput == "")
					{
						break;
					} else {
						//Collect user's input. Add current one to the list. 
						lines = lines.Concat(new string[] { userInput }).ToArray();
						
						//Use user's input to calculate the answer, every time the user adds new line. 
						timeData = new TextToTime(lines);
									
						//Distance between min and max values, in minutes
						minToMax = Convert.ToInt32((timeData.latest - timeData.earliest).TotalMinutes) + 1;
			
						//Create arrays that contain times (theTime) and number of busy drivers (busyTimes), using a custom class. 
						summary = new TimeAndDrivers(minToMax, timeData);
			
						//Find the answer from the array of drivers on break. 
						theAnswer = new GetAnswer(summary.busyTimes);			
			
						//Print the answer. Start and end of busiest period. 
						Console.WriteLine("The busiest period is " + summary.theTime[theAnswer.maxIndex] + "-" + summary.theTime[theAnswer.lastIndex-1] + ", with a total of " + theAnswer.maxValue + " drivers taking a break then.");
					
					}
				}
				
			} else if (args.Length == 0)
			{
				//Accept user input and calculate answer every time, until the user adds nothing.

				string userInput;
				string[] lines = new string[0];
			
				while (true)
				{
					Console.Write("Please input break time, format <start time><end time> : ");
					userInput = Console.ReadLine();
					//If user writes nothing, just pushes Enter, then exit.
					if (userInput == "")
					{
						break;
					} else {
						//Collect user's input. Add current one to the list. 
						lines = lines.Concat(new string[] { userInput }).ToArray();
						
						//Use user's input to calculate the answer, every time the user adds new line. 
						TextToTime timeData = new TextToTime(lines);
									
						//Distance between min and max values, in minutes
						int minToMax = Convert.ToInt32((timeData.latest - timeData.earliest).TotalMinutes) + 1;
			
						//Create arrays that contain times (theTime) and number of busy drivers (busyTimes), using a custom class. 
						TimeAndDrivers summary = new TimeAndDrivers(minToMax, timeData);
			
						//Find the answer from the array of drivers on break. 
						GetAnswer theAnswer = new GetAnswer(summary.busyTimes);			
			
						//Print the answer. Start and end of busiest period. 
						Console.WriteLine("The busiest period is " + summary.theTime[theAnswer.maxIndex] + "-" + summary.theTime[theAnswer.lastIndex-1] + ", with a total of " + theAnswer.maxValue + " drivers taking a break then.");
					
					}
				}
			} else 
			{
				Console.WriteLine("Error. Please either use two arguments - filename and [path to your file]. Or provide no command line arguments.");
			}
			
			
			Console.Write("Done. Press any key to exit . . . ");
			Console.ReadKey(true);
		}
	}
}