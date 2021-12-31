using UnityEngine;

public abstract class Gun : Item {
    public GameObject bulletPrefab;
    public abstract override void Use();
}