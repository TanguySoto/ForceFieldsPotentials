using System.Collections.Generic;

using FYFY;

/*
 * Project ISG : "Force Field Potentials"
 * UPMC 2017/2018
 * 
 * Nicolas BILLOD
 * Guillaume LORTHIOIR
 * Tanguy SOTO
 */

public class SystemsManager : FSystem {

	private static List<FSystem> systems = new List<FSystem>();

	public static void AddFSystem(FSystem f){
		systems.Add (f);
	}

	public static FSystem GetFSystem(string name){
		foreach (FSystem f in systems) {
			if (f.ToString ().Equals (name)) {
				return f;
			}
		}
		return null;
	}

	public static void ResetFSystems(){
		systems = new List<FSystem>();
	}
}
