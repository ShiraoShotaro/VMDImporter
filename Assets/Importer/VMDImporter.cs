using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using System;

[ScriptedImporter(1, "vmd")]
public class VMDImporter : ScriptedImporter
{
	private static string sjis2utf8(string sjis) {
		System.Text.Encoding src = System.Text.Encoding.GetEncoding(932);
		byte[] temp = src.GetBytes(sjis);
		return sjisbyte2utf8(temp);
	}

	private static string sjisbyte2utf8(byte[] sjis_byte)
	{
		System.Text.Encoding src = System.Text.Encoding.GetEncoding(932);
		System.Text.Encoding dst = System.Text.Encoding.UTF8;
		byte[] utf_temp = System.Text.Encoding.Convert(src, dst, sjis_byte);
		return dst.GetString(utf_temp);
	}

	private static string trimNull(string src)
	{
		int size = src.IndexOf('\0');
		return src.Substring(0, size);
	}

	private static void loadVMDHeader(FileStream vmd, out string fileSignature, out string modelName)
	{
		byte[] buf = new byte[128]; int readSize;
		
		// ファイルシグネチャ
		readSize = vmd.Read(buf, 0, 30);
		if (readSize != 30) throw new FormatException("Header Error (File Signature) - " + readSize.ToString() + " bytes.");
		fileSignature = System.Text.Encoding.ASCII.GetString(buf, 0, 30);
		Debug.Log("fileSignature = " + fileSignature);

		// モデル名
		readSize = vmd.Read(buf, 0, 20);
		if (readSize != 20) throw new FormatException("Header Error (Model Name) - " + readSize.ToString() + " bytes.");
		modelName = System.Text.Encoding.GetEncoding(932).GetString(buf, 0, 20);
		modelName = trimNull(sjis2utf8(modelName));
		Debug.Log("modelName = " + modelName);
	}

	private static void loadVMDMotionRecordSize(FileStream vmd, out int motionRecordSize)
	{
		byte[] buf = new byte[128]; int readSize;
		readSize = vmd.Read(buf, 0, 4);
		if (readSize != 4) throw new FormatException("Record Error (Record Num) - " + readSize.ToString() + " bytes.");
		motionRecordSize = BitConverter.ToInt32(buf, 0);
		Debug.Log("motionRecordSize = " + motionRecordSize.ToString());
	}

	private class MotionKey {
		private string boneName;
		private int frame;
		private Vector3 position;
		private Quaternion rotation;
		private Vector2 curveXa;
		private Vector2 curveYa;
		private Vector2 curveZa;
		private Vector2 curveRa;
		private Vector2 curveXb;
		private Vector2 curveYb;
		private Vector2 curveZb;
		private Vector2 curveRb;

		public string BoneName { get { return boneName; } }
		public int Frame { get { return frame; } }
		public Vector3 Position { get { return position; } }
		public Quaternion Rotation { get { return rotation; } }
		public Vector2 CurveXa { get { return curveXa; } }
		public Vector2 CurveYa { get { return curveYa; } }
		public Vector2 CurveZa { get { return curveZa; } }
		public Vector2 CurveXb { get { return curveXb; } }
		public Vector2 CurveYb { get { return curveYb; } }
		public Vector2 CurveZb { get { return curveZb; } }

		public static MotionKey loadMotion(FileStream vmd) {
			MotionKey key = new MotionKey();
			byte[] buf = new byte[16]; int readSize;
			
			readSize = vmd.Read(buf, 0, 15);
			if (readSize != 15) throw new FormatException("Key Error (Bone Name) - " + readSize.ToString() + " bytes.");
			key.boneName = System.Text.Encoding.GetEncoding(932).GetString(buf,0, 15);
			key.boneName = trimNull(sjis2utf8(key.boneName));

			readSize = vmd.Read(buf, 0, 4);
			if (readSize != 4) throw new FormatException("Key Error (Frame) - " + readSize.ToString() + " bytes.");
			key.frame = BitConverter.ToInt32(buf, 0);

			readSize = vmd.Read(buf, 0, 12);
			if (readSize != 12) throw new FormatException("Key Error (Position) - " + readSize.ToString() + " bytes.");
			key.position.x = BitConverter.ToInt32(buf, 0);
			key.position.y = BitConverter.ToInt32(buf, 4);
			key.position.z = BitConverter.ToInt32(buf, 8);

			readSize = vmd.Read(buf, 0, 16);
			if (readSize != 16) throw new FormatException("Key Error (Rotation) - " + readSize.ToString() + " bytes.");
			key.rotation.x = BitConverter.ToInt32(buf, 0);
			key.rotation.y = BitConverter.ToInt32(buf, 4);
			key.rotation.z = BitConverter.ToInt32(buf, 8);
			key.rotation.w = BitConverter.ToInt32(buf, 8);

			//TODO
			readSize = vmd.Read(buf, 0, 16);
			if (readSize != 16) throw new FormatException("Key Error (curve1) - " + readSize.ToString() + " bytes.");
			readSize = vmd.Read(buf, 0, 16);
			if (readSize != 16) throw new FormatException("Key Error (curve2) - " + readSize.ToString() + " bytes.");
			readSize = vmd.Read(buf, 0, 16);
			if (readSize != 16) throw new FormatException("Key Error (curve3) - " + readSize.ToString() + " bytes.");
			readSize = vmd.Read(buf, 0, 16);
			if (readSize != 16) throw new FormatException("Key Error (curve4) - " + readSize.ToString() + " bytes.");

			Debug.Log("Frame = " + key.frame + ", BoneName = " + key.boneName);

			return key;
		}

	};

	private static void createCurve_Transform(List<MotionKey> keys, string propertyName, ref AnimationClip clip) {
		AnimationCurve curveX = new AnimationCurve();
		AnimationCurve curveY = new AnimationCurve();
		AnimationCurve curveZ = new AnimationCurve();

		foreach (MotionKey key in keys)
		{
			curveX.AddKey(new Keyframe(key.Frame / 60f, key.Position.x));
			curveY.AddKey(new Keyframe(key.Frame / 60f, key.Position.y));
			curveZ.AddKey(new Keyframe(key.Frame / 60f, key.Position.z));
		}

		clip.SetCurve("Root", typeof(Animator), propertyName + ".x", curveX);
		clip.SetCurve("", typeof(Animator), propertyName + ".y", curveY);
		clip.SetCurve("", typeof(Animator), propertyName + ".z", curveZ);
	}


	private static AnimationClip loadVMD(FileStream vmd) {
		AnimationClip ret = new AnimationClip();

		

		// ヘッダー
		string fileSignature, modelName;
		loadVMDHeader(vmd, out fileSignature, out modelName);

		// モーションレコード数	
		int motionRecordSize;
		loadVMDMotionRecordSize(vmd, out motionRecordSize);

		// ループ処理
		Dictionary<string, List<MotionKey>> bone_key = new Dictionary<string, List<MotionKey>>();
		for (int i = 0; i < motionRecordSize; ++i) {
			MotionKey key = MotionKey.loadMotion(vmd);
			if (key.BoneName == null || key.BoneName == "") {
				Debug.LogWarning("Bone name is null or empty!! Skipped.");
				continue;
			}
			if (!bone_key.ContainsKey(key.BoneName)) bone_key.Add(key.BoneName, new List<MotionKey>());
			bone_key[key.BoneName].Add(key);
		}

		//TODO:カメラのモーションも読み込み？

		//読出し終了
		
		foreach (KeyValuePair<string, List<MotionKey>> keys in bone_key) {
			switch (keys.Key) {
				case "全ての親":
					createCurve_Transform(keys.Value, "Motion T", ref ret);
					Debug.Log("Added 全ての親");
                    break;
			}
		}
		



		return ret;
	}

	public override void OnImportAsset(AssetImportContext ctx)
	{
		UnityEditor.EditorUtility.DisplayDialog(
			"VMD Loader",
			ctx.assetPath
				+ "\nUnityへ読み込む前に、必ずモーションデータの規約をご確認ください。\n"
				+ "また当インポータを使用して生じた問題について、開発者は一切の責任を負いかねます。"
				+ "詳しい内容は、VMDImporterに関する規約をご確認ください。\n"
				+ "（VMDImporter : http://localhost/test.html）",
			"OK"
		);
		
		AnimationClip animationClip = loadVMD(new FileStream(ctx.assetPath, FileMode.Open, FileAccess.Read));
		ctx.AddObjectToAsset("VMDMotion", animationClip);
		ctx.SetMainObject(animationClip);
		AssetDatabase.CreateAsset(animationClip, "Assets/Test.anim");
	}
}
