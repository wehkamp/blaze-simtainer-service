using Blaze.SimTainer.Service.Api.Dtos.Game;
using System.Collections.Generic;

namespace Blaze.SimTainer.Service.Api.Interfaces
{
	internal interface IDataConverter<T>
	{
		public GameDto GenerateGameDto(ICollection<T> list);
	}
}