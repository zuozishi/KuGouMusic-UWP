using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KuGouMusicUWP.Class
{
    public class DataCenter
    {
        public async Task WriteAll(List<object> data, string Path)
        {
            var jsondata = new List<string>();
            foreach (var item in data)
            {
                jsondata.Add(Class.data.ToJsonData(item));
            }
            var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri(Path));
            await Windows.Storage.FileIO.WriteLinesAsync(file, jsondata);
        }
        public async Task AddObj(object data, string Path)
        {
            var jsondata = Class.data.ToJsonData(data);
            var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri(Path));
            var datas= await Windows.Storage.FileIO.ReadLinesAsync(file);
            datas.Add(jsondata);
            await Windows.Storage.FileIO.WriteLinesAsync(file, datas);
        }
        public async Task<List<T>> GetObjs<T>(string Path)
        {
            var result = new List<T>();
            var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri(Path));
            var datas = await Windows.Storage.FileIO.ReadLinesAsync(file);
            foreach (var item in datas)
            {
                result.Add(Class.data.DataContractJsonDeSerialize<T>(item));
            }
            return result;
        }
        public async Task SaveObjs<T>(List<T> data,string Path)
        {
            var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri(Path));
            var result = new List<string>();
            foreach (var item in data)
            {
                result.Add(Class.data.ToJsonData(item));
            }
            await Windows.Storage.FileIO.WriteLinesAsync(file, result);
        }
    }
}
