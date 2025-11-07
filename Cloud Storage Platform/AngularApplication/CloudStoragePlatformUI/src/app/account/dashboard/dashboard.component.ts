import {Component, OnDestroy, OnInit, AfterViewInit, ElementRef, ViewChild} from '@angular/core';
import {FilesStateService} from "../../services/StateManagementServices/files-state.service";
import {FormBuilder, FormGroup, Validators} from '@angular/forms';
import { phoneNumberValidator } from '../../Utils';
import { AccountService, AccountDetailsAndAnalytics } from '../../services/ApiServices/account.service';
import {Router} from "@angular/router";

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit, OnDestroy, AfterViewInit {
  @ViewChild('analyticsContainer', { static: false }) analyticsContainer!: ElementRef;
  chartView: [number, number] = [400, 400];
  barChartView: [number, number] = [500, 300];
  private resizeObserver!: ResizeObserver;

  updateAccountMode = false;
  updateForm!: FormGroup;
  countries: string[] = [
    'United States', 'Canada', 'United Kingdom', 'Australia', 'Germany',
    'France', 'Spain', 'Italy', 'Japan', 'China', 'India', 'Brazil',
    'Mexico', 'Russia', 'South Africa'
  ];
  deleteConfirmMode = false;
  accountDetails: AccountDetailsAndAnalytics | null = null;
  loadingAnalytics = true;
  updatednow = false;
  showPassword = false;

  constructor(private filesState:FilesStateService, private fb: FormBuilder, private accountService: AccountService, private router:Router) {}
  pieData:any = [
  ];
  barData:any = [
  ];


  ngOnInit(): void {
    this.filesState.outsideFilesAndFoldersMode = true;
    this.filesState.showSpaceUtilizedInNavDrawer = false;
    this.loadingAnalytics = true;
    this.accountService.getAccountDetailsAndAnalytics().subscribe({
      next: (data) => {
        this.accountDetails = data;
        this.loadingAnalytics = false;
        // Update chart data
        console.log(data.topExtensionsBySize, data.topFilesBySize);
        this.pieData = data.topExtensionsBySize.map(e => ({ name: e.extension, value: e.totalSize }));
        this.barData = data.topFilesBySize.map(f => ({ name: f.fileName, value: f.size }));
        // Set form values from API
        this.updateForm.setValue({
          email: data.email || '',
          fullName: data.personName || '',
          country: data.country || '',
          phoneNumber: data.phoneNumber || '',
          password: '',
          confirmPassword: ''
        });
        if (this.updateForm.get('phoneNumber')?.value=="N/A"){
          this.updateForm.get('phoneNumber')?.setValue('');
        }
      },
      error: () => {
        this.loadingAnalytics = false;
      }
    });
    this.updateForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      fullName: ['', [Validators.required]],
      country: ['', [Validators.required]],
      phoneNumber: ['', [phoneNumberValidator]],
      password: ['', [Validators.minLength(6)]],
      confirmPassword: ['']
    }, { validators: this.passwordMatchValidator });
    // Clear emailAlreadyRegistered error when email changes
    this.updateForm.get('email')?.valueChanges.subscribe(() => {
      const errors = this.updateForm.get('email')?.errors;
      if (errors && errors['emailAlreadyRegistered']) {
        const { emailAlreadyRegistered, ...rest } = errors;
        this.updateForm.get('email')?.setErrors(Object.keys(rest).length ? rest : null);
      }
    });
  }

  onUpdateDetailsClick() {
    this.updateAccountMode = true;
  }

  onCancelUpdate() {
    this.updateAccountMode = false;
    if (this.accountDetails) {
      this.updateForm.reset({
        email: this.accountDetails.email || '',
        fullName: this.accountDetails.personName || '',
        country: this.accountDetails.country || '',
        phoneNumber: this.accountDetails.phoneNumber || '',
        password: '',
        confirmPassword: ''
      });
      if (this.updateForm.get('phoneNumber')?.value=="N/A"){
        this.updateForm.get('phoneNumber')?.setValue('');
      }
    } else {
      this.updateForm.reset({
        email: '',
        fullName: '',
        country: '',
        phoneNumber: '',
        password: '',
        confirmPassword: ''
      });
    }
  }

  onConfirmUpdate() {
    if (this.updateForm.valid && this.accountDetails) {
      // Prepare DTO with only changed values
      const formValue = this.updateForm.value;
      const dto: any = {};
      if (formValue.email && formValue.email !== this.accountDetails.email) dto.email = formValue.email;
      else dto.email = null;
      if (formValue.fullName && formValue.fullName !== this.accountDetails.personName) dto.fullName = formValue.fullName;
      else dto.fullName = null;
      if (formValue.country && formValue.country !== this.accountDetails.country) dto.country = formValue.country;
      else dto.country = null;
      if (formValue.phoneNumber && formValue.phoneNumber !== this.accountDetails.phoneNumber && formValue.phoneNumber !== 'N/A') dto.phoneNumber = formValue.phoneNumber;
      else dto.phoneNumber = null;
      if (formValue.password) dto.password = formValue.password;
      else dto.password = null;
      if (formValue.confirmPassword) dto.confirmPassword = formValue.confirmPassword;
      else dto.confirmPassword = null;
      this.loadingAnalytics = true;
      this.accountService.updateAccount(dto).subscribe({
        next: (updated) => {
          // Update local data
          this.accountDetails = { ...this.accountDetails!, ...updated };
          this.updateAccountMode = false;
          this.loadingAnalytics = false;
          // Reset form to new values
          this.updateForm.reset({
            email: this.accountDetails?.email || '',
            fullName: this.accountDetails?.personName || '',
            country: this.accountDetails?.country || '',
            phoneNumber: this.accountDetails?.phoneNumber || '',
            password: '',
            confirmPassword: ''
          });
          this.updatednow = true;
          setTimeout(() => this.updatednow=false, 5000)
        },
        error: (err) => {
          this.loadingAnalytics = false;
          if (err?.error && err.error.detail.includes('already taken')) {
            this.updateForm.get('email')?.setErrors({ ...this.updateForm.get('email')?.errors, emailAlreadyRegistered: true });
          }
        }
      });
    } else {
      this.updateForm.markAllAsTouched();
    }
  }

  passwordMatchValidator(form: FormGroup) {
    const password = form.get('password')?.value;
    const confirmPassword = form.get('confirmPassword')?.value;
    if (password !== confirmPassword) {
      form.get('confirmPassword')?.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    } else {
      return null;
    }
  }

  onDeleteAccountClick() {
    this.deleteConfirmMode = true;
  }
  onCancelDelete() {
    this.deleteConfirmMode = false;
  }
  onConfirmDelete() {
    this.accountService.deleteAccount().subscribe({
      next:()=>{
        alert("This account has been successfully deleted.");
      },
      error:()=>{
        alert("Failed to delete the account because of an error.");
      }
    });
    this.deleteConfirmMode = false;
  }

  ngAfterViewInit(): void {
    this.setChartSizes();
    this.resizeObserver = new ResizeObserver(() => {
      this.setChartSizes();
    });
    if (this.analyticsContainer && this.analyticsContainer.nativeElement) {
      this.resizeObserver.observe(this.analyticsContainer.nativeElement);
    }
    window.addEventListener('resize', this.setChartSizes.bind(this));
  }

  setChartSizes() {
    let width = window.innerWidth;
    let pieSize = 400;
    let barWidth = 500;
    let barHeight = 300;
    if (width < 1186 && width > 854) {
      pieSize = 260;
      barWidth = 320;
    }
    if (width < 856) {
      pieSize = 240;
      barWidth = 250;
      barHeight = 250;
    }
    this.chartView = [pieSize, pieSize];
    this.barChartView = [barWidth, barHeight];
  }

  ngOnDestroy(): void {
    this.filesState.outsideFilesAndFoldersMode = false;
    this.filesState.showSpaceUtilizedInNavDrawer = true;
    if (this.resizeObserver && this.analyticsContainer && this.analyticsContainer.nativeElement) {
      this.resizeObserver.unobserve(this.analyticsContainer.nativeElement);
    }
    window.removeEventListener('resize', this.setChartSizes.bind(this));
  }

  logout() {
    this.accountService.logout().subscribe({
      next:()=>{
        this.router.navigate(["account","login"]);
      }
    })
  }
}
