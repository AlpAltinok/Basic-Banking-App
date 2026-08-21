namespace BankaApp.Application.Interfaces;

/// <summary>
/// Unit of Work: birden fazla repository değişikliğini tutarlı kaydetmek için.
/// Transfer gibi işlemlerde atomiklik (hepsi olur ya da hiçbiri) kritiktir.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// İşlemi DB transaction içinde çalıştırır; hata olursa Rollback.
    /// </summary>
    Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default);

    Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default);
}
