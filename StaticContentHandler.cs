using System;
using System.Web;
using System.IO;
using System.Collections;
using System.ComponentModel; // EventHandlerList

namespace IIS7Demos {
	class StaticContentHandler : IHttpHandler {

		public Hashtable mimeMap = new Hashtable();

		private void Init(){
			mimeMap.Add("png","image/png");
			mimeMap.Add("jpg","image/jpg");
			mimeMap.Add("jpeg","image/jpeg");
			mimeMap.Add("gif","image/gif");
			mimeMap.Add("php","text/php");
			mimeMap.Add("html","text/html");
			mimeMap.Add("css","text/css");
			mimeMap.Add("txt","text/plain");
			mimeMap.Add("json","text/json");
			mimeMap.Add("js","application/javascript");
			mimeMap.Add("mp4","video/mp4");
			mimeMap.Add("m4a","audio/m4a");
			mimeMap.Add("mp3","audio/mp3");
		}

		private void OnError(Object o, EventArgs args){
			HttpApplication app = (HttpApplication)o;
			HttpContext context = app.Context;

		    Exception exc = app.Server.GetLastError();

		    // Handle specific exception.
		    if (exc is HttpUnhandledException){
		        context.Response.Write("An error occurred on this page. Please verify your " +                  
		        	"information to resolve the issue.");
		    }
		    // Clear the error from the server.
		    app.Server.ClearError();
		}
		
		#region IHttpHandler Members

		public bool IsReusable {
			get { return true; }
		}

		public void ProcessRequest(HttpContext context){
			try{this.Init();}catch(Exception e){}

			HttpApplication app = context.ApplicationInstance;
			HttpResponse response = context.Response;
			HttpRequest request = context.Request;

			// if(response.StatusCode == 404.0){
			// 	response.Redirect("../");
			// }

			string path = request.Path;
			string path2 = request.PhysicalPath;
	    	string tempBasePath = context.Request.AppRelativeCurrentExecutionFilePath;
	    	string url = request.RawUrl;

    		int si = url.IndexOf("/") + 1;
    		string basePath = tempBasePath.Substring(si,tempBasePath.Length - si);

    		bool hasSubFolder = false;

			try{
				string[] dirs = Directory.GetDirectories(path2);
				string[] files = Directory.GetFiles(path2);
		    	string[] items = new string[files.Length + dirs.Length];
		    	dirs.CopyTo(items,0);
		    	files.CopyTo(items,dirs.Length);

				// Check for default document containing "default" or "index"
				if(File.GetAttributes(path2).HasFlag(FileAttributes.Directory)){
					context.Response.Write(String.Format("<title>Contents of /{0}</title>",basePath));

					foreach(string file in files){
						int startIndex = file.LastIndexOf("\\") + 1;
						string filename = file.Substring(startIndex,file.Length - startIndex);

						if(filename.IndexOf("default") != -1 || filename.IndexOf("index") != -1){
				    		response.Write("Hello");
							response.Redirect(basePath + "/" + filename);
						}
					}
				}

		    	context.Response.Write("<style>.wrapper > h1 { text-align: center; } "+
		    		"ul { display: grid; grid-auto-cols: 1fr; grid-gap: 10px; list-style:none; }"+
		    		"ul > li { display: grid; grid-template-columns: 60px auto 60px; line-height: 30px; font-size: 20px; font-family: monospace; }"+
		    		"ul > li > .icon { width: 30px; height: 30px; }"+
		    		"a.download-button { text-decoration: none }"+
		    		"a.download-button > div { width: 30px; height: 30px; background-image: url('data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAB4AAAAeCAAAAAAeW/F+AAAABGdBTUEAALGPC/xhBQAAACBjSFJNAAB6JgAAgIQAAPoAAACA6AAAdTAAAOpgAAA6mAAAF3CculE8AAAAAmJLR0QA/4ePzL8AAAAHdElNRQfiBxsKEjmIBmGqAAAAzElEQVQoz2P4jxcwDBXpz2/efMIt/bEoICD/PU7plz76+l7PcEv7GRr60ET6fnNtqa2RkU1JbfN9LNJ3A3QNjIHAQDfgLjbDzwdBpIPOY7cbLI8ii+q0i0EGBkEXsTjt1c4tQHDvYnDwxXsg1o5XKNKzjYC2GoVdefToShiYORVFulcf5CrDwMVLgwxBLP02FOk+sKONDQ2NDI1aahwWBrai6tY1hAGDkhzrHg9U6d2R4XAQGhoWErENRfrvx/cfkMD7j3+x+BsTDGJpAPNAEhU+GhSmAAAAJXRFWHRkYXRlOmNyZWF0ZQAyMDE4LTA3LTI3VDEwOjE4OjU3LTA0OjAwO+ug1AAAACV0RVh0ZGF0ZTptb2RpZnkAMjAxOC0wNy0yN1QxMDoxODo1Ny0wNDowMEq2GGgAAAAASUVORK5CYII='); }"+
		    		"ul > li > .icon.file { background-image: url('data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAB4AAAAeCAAAAAAeW/F+AAAABGdBTUEAALGPC/xhBQAAACBjSFJNAAB6JgAAgIQAAPoAAACA6AAAdTAAAOpgAAA6mAAAF3CculE8AAAAAmJLR0QA/4ePzL8AAAAHdElNRQfiBxoUBxxas7rmAAAA00lEQVQoz2P4DwR/j6TbGMOARcn9/3DAACJ2uuobmUKAmYmRUeJ1FOnHgfoRy3dDwN7lrkYG4VeQpTcYuV2Gq/+ZY2BsEH4VSXqabup3uPTfMgNjY4OIqwjpSbqZPxHS5UBpY5j5OKSNDSJvY5cu0wf7zyDzHTbp/9MtgcDC2MhkA1bpT2ePHz9+dKGz/mSs0hDwPU13Eh7pn5mj0oNXGiUpwiI0VXfafywJGQouuxnBUgtyNoCC5RH6gY//Y2YiKDDSd935HyaNkgXBwCb9yF+QDAAYJMjYnOJttAAAACV0RVh0ZGF0ZTpjcmVhdGUAMjAxOC0wNy0yNlQyMDowNzoyOC0wNDowMCXq6SMAAAAldEVYdGRhdGU6bW9kaWZ5ADIwMTgtMDctMjZUMjA6MDc6MjgtMDQ6MDBUt1GfAAAAAElFTkSuQmCC'); }"+
		    		"ul > li > .icon.folder { background-image: url('data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAB4AAAAeCAMAAAAM7l6QAAAABGdBTUEAALGPC/xhBQAAACBjSFJNAAB6JgAAgIQAAPoAAACA6AAAdTAAAOpgAAA6mAAAF3CculE8AAABsFBMVEX////////7/f75/P35/P35/P38/f73+vyrzeNuqc9npc1opc5npc18sdTU5fH+//+rzeMtg7olfrgmfrglfrglfrhBjsGiyODz+Pv7/f5uqc8lfrgogLkof7kof7kof7kmfrgqgbprp8651ejB2uvA2erA2erA2erA2erG3ezr8/n5/P1npc0of7g5h7o8iLs8iLs8iLs7iLs6h7o/irxAi7xAi7xAi7xAi7xAi7xAi7w/i7xdnsnZ6PL5+/12rM97pcGlucaousaousaousaousaousaousaousaluMZ6pMCRu9f+///j8PlsrtpcpNVdpdZdpdVdpdVdpdVdpdVdpdVdpdVcpNVtrtrj8Plyt+U0mNsyl9syl9syl9syl9s0mNtyt+U3mds0mNs0mNs0mNs0mNs3mds0mNs0mNs0mNs0mNs0mNs0mNszl9k0mNs0mNs0mNszltk0j8wzl9k0mNw0mNs0mNs0mNwzl9kzj8xvqtErhsMriskriskriskriskrhsNwqtHp8veWwNxqps5npc1opc1npc1qps6WwNzp8vj+///6/P35/P35/P1CFk62AAAAAWJLR0QAiAUdSAAAAAd0SU1FB+IHGhQLFPjdfdgAAADuSURBVCjPY2BgYGRiZgEDVjYGTMDOwcnFDQY8vHz8GNICgkLCECAiKiYugSYrKSUtIysHAfIKikrKKqpq6mpArKGpBZTW1tHV0zeAAkMjYxNTM3MLczNLCytrG1sGBjt7B0cnZzhwcXVzBwM3D08vbx8GXz//gMCg4JCgYCAICgVRYBwSFhAeEckQFR0TG4cVxMYnJDIkJaek4gApaekMGZmpOEFmFkMqXjAqPfiks3NwS+bkMuTlF+CSLSgsYiguKS0rxwrKKiqrGKprauvqsYK6hsYmhuaW1rZ2rKCjs6ubgaGnt68fK+jr7WEAAKyWOdykZ8pwAAAAJXRFWHRkYXRlOmNyZWF0ZQAyMDE4LTA3LTI2VDIwOjExOjIwLTA0OjAw2pUIwwAAACV0RVh0ZGF0ZTptb2RpZnkAMjAxOC0wNy0yNlQyMDoxMToyMC0wNDowMKvIsH8AAAAASUVORK5CYII='); }</style>");

		    	context.Response.Write(String.Format("<div class='wrapper'><hr><h1>{0}</h1><hr><ul>",
		    		url.Substring(1,url.Length-1)));

		    	for (int i = 0; i < items.Length; i++){
		    		string item = items[i];
		    		int startIndex = item.LastIndexOf("\\") + 1;
		    		string name = item.Substring(startIndex,item.Length - startIndex);

		    		if(name == "web.config" ||
		    			name.ToLower() == "bin") continue;

		    		string type;
		    		string download = "";
		    		if(i >= dirs.Length) {
		    			type = "file";
		    			download = String.Format("<div><a class='download-button' href='{2}{0}' download='{0}'><div></div></a></div>",name,type,basePath);
		    		} else {
		    			type = "folder";
		    		}

		    		context.Response.Write(String.Format("<li><div class='icon {1}'></div>"+
		    			"<div><a href='{2}{0}'>{0}</a></div>"+
		    			"{3}</li>",name,type,basePath,download));
		    	}
		    	context.Response.Write("</ul></div>");

				if(items.Length > 0) hasSubFolder = true;
			}catch(Exception e){
				int dotPos = path2.LastIndexOf(".")+1;
				int slashPos = path2.LastIndexOf("\\")+1;
				string name = path2.Substring(slashPos,path2.Length - slashPos);
				if(dotPos == 0){ // Didn't have extensions
					using (StreamReader sr = new StreamReader(path2)){
		                String line = sr.ReadToEnd();
						response.Write(line);
		            }
				}else{	// Have extension
					string ext = path2.Substring(dotPos,path2.Length - dotPos);
					bool hasMime = false;
					string mime = "";
					foreach(string key in mimeMap.Keys){
						if(name.IndexOf(key) != -1){
							response.AddHeader("Content-Type",(string)mimeMap[key]);
							hasMime = true;
							mime = (string)mimeMap[key];
						}
					}
					if(hasMime){
						if(mime.IndexOf("text/") >= 0){
							using (StreamReader sr = new StreamReader(path2)){
				                String line = sr.ReadToEnd();
								response.Write(line);
				            }
				        }else{
			                byte[] bytes = File.ReadAllBytes(path2);
							response.AddHeader("Content-Type",mime);
							response.BinaryWrite(bytes);
				        }
					}else{
						if(hasSubFolder)response.Write(String.Format("No Mime Map for {0}<br>",name));
					}
				}
			}
		}

		#endregion

	}
}