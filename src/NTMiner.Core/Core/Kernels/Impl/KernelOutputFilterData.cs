﻿using System;

namespace NTMiner.Core.Kernels.Impl {
    public class KernelOutputFilterData : IKernelOutputFilter, IDbEntity<Guid> {
        public Guid GetId() {
            return this.Id;
        }

        public Guid Id { get; set; }

        public Guid KernelId { get; set; }

        public string RegexPattern { get; set; }
    }
}
