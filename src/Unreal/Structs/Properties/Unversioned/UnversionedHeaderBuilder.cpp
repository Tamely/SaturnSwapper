import Saturn.Unversioned.UnversionedHeaderBuilder;

import Saturn.Unversioned.UnversionedHeader;
import Saturn.Unversioned.Fragment;

FUnversionedHeaderBuilder::FUnversionedHeaderBuilder() {
    Fragments.push_back(FFragment());
}

void FUnversionedHeaderBuilder::IncludeProperty(bool bIsZero) {
    if (Fragments.back().ValueNum == FFragment::ValueMax) {
        TrimZeroMask(Fragments.back());
        Fragments.push_back(FFragment());
    }

    ++Fragments.back().ValueNum;
    Fragments.back().bHasAnyZeroes |= bIsZero;
    ZeroMask.push_back(bIsZero);
    bHasNonZeroValues |= !bIsZero;
}

void FUnversionedHeaderBuilder::ExcludeProperty() {
    if (Fragments.back().ValueNum || Fragments.back().SkipNum == FFragment::SkipMax) {
        TrimZeroMask(Fragments.back());
        Fragments.push_back(FFragment());
    }

    ++Fragments.back().SkipNum;
}

void FUnversionedHeaderBuilder::Finalize() {
    TrimZeroMask(Fragments.back());

    // Trim trailing skips
    while (Fragments.size() > 1 && Fragments.back().ValueNum == 0) {
        Fragments.pop_back();
    }

    Fragments.back().bIsLast = true;
}

void FUnversionedHeaderBuilder::TrimZeroMask(const FFragment& Fragment) {
    if (!Fragment.bHasAnyZeroes) {
        ZeroMask.erase(ZeroMask.end() - Fragment.ValueNum, ZeroMask.end());
    }
}
