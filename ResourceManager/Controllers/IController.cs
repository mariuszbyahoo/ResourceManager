using ResourceManager.Domain;
using ResourceManager.Domain.Interfaces;
using System;

namespace ResourceManager
{
	/// <summary>
	/// Kontroler zasobów
	/// </summary>
	public interface IController
	{
		/// <summary>
		/// Dodanie nowego zasobu do puli
		/// </summary>
		void AddResource(IResource resource, DateTime fromDate);

		/// <summary>
		/// Wycofanie zasobu z puli
		/// </summary>
		void WithdrawResource(IResource resource, DateTime fromDate);

		/// <summary>
		/// Wydzierżawienie konkretnego zasobu na określony dzień  
		/// </summary>
		bool LeaseResource(IResource resource, ITenant tenant, DateTime date);

		/// <summary>
		/// Wydzierżawienie zasobu w określonym wariancie na określony dzień  
		/// </summary>
		bool LeaseResource(string variant, ITenant tenant, DateTime date, out IResource resource);

		/// <summary>
		/// Zwolnienie zasobu, jeżeli jest dzierżawiony 
		/// </summary>
		bool FreeResource(IResource resource, ITenant tenant, DateTime date);
	}
}