using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System;

public class PhotoJsonData
{
    public int total { get; set; }
    public int total_pages { get; set; }
    public List<Result> results { get; set; }
}

public class Urls
{
    public string raw { get; set; }
    public string full { get; set; }
    public string regular { get; set; }
    public string small { get; set; }
    public string thumb { get; set; }
}

public class Links
{
    public string self { get; set; }
    public string html { get; set; }
    public string download { get; set; }
    public string download_location { get; set; }
    public string photos { get; set; }
    public string likes { get; set; }
    public string portfolio { get; set; }
    public string following { get; set; }
    public string followers { get; set; }
}

public class ProfileImage
{
    public string small { get; set; }
    public string medium { get; set; }
    public string large { get; set; }
}

public class Social
{
    public string instagram_username { get; set; }
    public string portfolio_url { get; set; }
    public string twitter_username { get; set; }
    public object paypal_email { get; set; }
}

public class User
{
    public string id { get; set; }
    public DateTime updated_at { get; set; }
    public string username { get; set; }
    public string name { get; set; }
    public string first_name { get; set; }
    public string last_name { get; set; }
    public string twitter_username { get; set; }
    public string portfolio_url { get; set; }
    public string bio { get; set; }
    public string location { get; set; }
    public Links links { get; set; }
    public ProfileImage profile_image { get; set; }
    public string instagram_username { get; set; }
    public int total_collections { get; set; }
    public int total_likes { get; set; }
    public int total_photos { get; set; }
    public bool accepted_tos { get; set; }
    public bool for_hire { get; set; }
    public Social social { get; set; }
}

public class Type
{
    public string slug { get; set; }
    public string pretty_slug { get; set; }
}

public class Category
{
    public string slug { get; set; }
    public string pretty_slug { get; set; }
}

public class Subcategory
{
    public string slug { get; set; }
    public string pretty_slug { get; set; }
}

public class Ancestry
{
    public Type type { get; set; }
    public Category category { get; set; }
    public Subcategory subcategory { get; set; }
}

public class CoverPhoto
{
    public string id { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
    public DateTime promoted_at { get; set; }
    public int width { get; set; }
    public int height { get; set; }
    public string color { get; set; }
    public string blur_hash { get; set; }
    public string description { get; set; }
    public string alt_description { get; set; }
    public Urls urls { get; set; }
    public Links links { get; set; }
    public List<object> categories { get; set; }
    public int likes { get; set; }
    public bool liked_by_user { get; set; }
    public List<object> current_user_collections { get; set; }
    public object sponsorship { get; set; }
    public User user { get; set; }
}

public class Source
{
    public Ancestry ancestry { get; set; }
    public string title { get; set; }
    public string subtitle { get; set; }
    public string description { get; set; }
    public string meta_title { get; set; }
    public string meta_description { get; set; }
    public CoverPhoto cover_photo { get; set; }
}

public class Tag
{
    public string type { get; set; }
    public string title { get; set; }
    public Source source { get; set; }
}

public class Result
{
    public string id { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
    public DateTime? promoted_at { get; set; }
    public int width { get; set; }
    public int height { get; set; }
    public string color { get; set; }
    public string blur_hash { get; set; }
    public string description { get; set; }
    public string alt_description { get; set; }
    public Urls urls { get; set; }
    public Links links { get; set; }
    public List<object> categories { get; set; }
    public int likes { get; set; }
    public bool liked_by_user { get; set; }
    public List<object> current_user_collections { get; set; }
    public object sponsorship { get; set; }
    public User user { get; set; }
    public List<Tag> tags { get; set; }
}


public class Search : MonoBehaviour
{
    [SerializeField] RawImage itemPrefab;
    [SerializeField] Button searchButton;
    [SerializeField] InputField textinput;
    [SerializeField] Transform photoContainer;

    private void Awake()
    {
        searchButton.onClick.AddListener(() => OnClickSearch(textinput.text).Forget());
    }

    //async UniTaskVoid 가 async void보다 호환이 좋음.
    //내용에서 await를 사용할 일이없으면 UniTaskVoid 사용할 일이 있으면 UniTask
    private async UniTaskVoid OnClickSearch(string term)
    {
        //이전 이미지 삭제
        var beforeImages = photoContainer.GetComponentsInChildren<RawImage>();
        foreach (RawImage photo in beforeImages)
        {
            Destroy(photo.gameObject);
        }

        //검색한 data가 들어있는 url 데이터를 비동기로 받아옴
        //데이터가 넘어올때까지 기다렸다가 다음 작업
        var photosData = await GetPhotosData(term);
        if(photosData == null)
        {
            Debug.LogWarning("photos data is null");
            return;
        }

        //받아온 url데이터 처리
        var photos = JsonConvert.DeserializeObject<PhotoJsonData>(photosData);
        foreach(var photo in photos.results)
        {
            var url = photo.urls.regular;
            //해당 url에서 이미지를 가져옴.
            //async 함수를 await할 필요가 없을때는 Forget을 사용한다.
            LoadAndShowPhoto(url).Forget();
        }
    }

    //검색데이터를 받아옴
    //async await 를 사용할때 최상단에 기본적으로 try catch 를 사용함.
    async UniTask<string> GetPhotosData(string term)
    {
        //using = 관리되지 않는 리소스에대한 자원 해제(IDisposable)를 대신해주는 것 = 메모리정리
        using var req = UnityWebRequest.Get($"https://api.unsplash.com/search/photos?query={term}");
        try
        {
            //데이터 입력
            req.SetRequestHeader("Authorization", "Client-ID S2F1QMER5i40nOj6vV_JUrGfK7e2l7Ue0f_MLUX5STQ");
            //통신시작
            await req.SendWebRequest();

            return req.downloadHandler.text;
        }
        catch (Exception err)
        {
            Debug.LogError("GetPhotosData: " + err);
            return null;
        }
    }

    //url데이터로부터 이미지를 가져옴
    async UniTask LoadAndShowPhoto(string url)
    {
        try
        {
            var texture = await GetTexture(url);
            if (texture == null)
            {
                Debug.LogWarning("texture is null");
                return;
            }

            var obj = Instantiate(itemPrefab, photoContainer);
            obj.GetComponent<RawImage>().texture = texture;
        }
        catch(Exception err)
        {
            Debug.LogError("LoadAndShowPhoto: " + err);
            return;
        }
        
    }

    private async UniTask<Texture> GetTexture(string url)
    { 
        using(var req = UnityWebRequestTexture.GetTexture(url))
        {
            try
            {
                await req.SendWebRequest();
                return DownloadHandlerTexture.GetContent(req);
            }
            catch(Exception err)
            {
                Debug.LogError("GetTexture error: " + err);
                return null;
            }
        }
    }
}
