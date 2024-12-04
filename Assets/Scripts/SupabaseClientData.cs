using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SupabaseClientData", menuName = "ScriptableObjects/SupabaseClientData")]
public class SupabaseClientData : ScriptableObject
{
    private Supabase.Client _client = null;

    public Supabase.Client Client
    {
        get => _client;
        set => _client = value;
    }

    public void ResetClient()
    {
        _client = null;
    }
}

