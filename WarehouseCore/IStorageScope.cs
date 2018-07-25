namespace WarehouseCore
{
	public interface IStorageScope
	{
		string ScopeName { get; }

		string Identifier { get; }
	}
}