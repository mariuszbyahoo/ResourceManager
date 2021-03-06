﻿using ResourceManager.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResourceManager.Domain.Models
{
	/// <summary>
	/// Zasób
	/// </summary>
	public interface IResource
	{
		/// <summary>
		/// Identyfikator zasobu
		/// </summary>
		Guid Id { get; set; }

		/// <summary>
		/// Wariant zasobu
		/// </summary>
		string Variant { get; set; }
	}
}
