using System;
using CommunityToolkit.Mvvm.ComponentModel;
using dawn.ftp.ui.BusinessLogic;

namespace dawn.ftp.ui.Models.FileModels
{
    public class FileSystemObjectModel : ObservableObject, IModelWrapper<FileModel>, IModelWrapper<FileSystemObjectModel>
    {
        private int _id;
        private FileModel _model;
        private FileSystemObjectModel _model1;

        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        FileModel IModelWrapper<FileModel>.Model => _model;

        public static IModelWrapper<FileSystemObjectModel> WrapModel(FileSystemObjectModel model)
        {
            throw new NotImplementedException();
        }

        public static IModelWrapper<FileSystemObjectModel> WrapNewModel(FileSystemObjectModel model)
        {
            throw new NotImplementedException();
        }

        public static IModelWrapper<FileModel> WrapModel(FileModel model)
        {
            throw new NotImplementedException();
        }

        public static IModelWrapper<FileModel> WrapNewModel(FileModel model)
        {
            throw new NotImplementedException();
        }

        FileSystemObjectModel IModelWrapper<FileSystemObjectModel>.Model => _model1;
    }
}
